using UnityEngine;
using UnityEngine.AI;

public class MechaBoBMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private MechaBoBAttack attack;

    [Header("Patrol")]
    [SerializeField] private Transform patrolRoute;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private bool randomPatrol = false;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Chase")]
    [SerializeField] private float chaseRange = 20f;
    [SerializeField] private float loseRange = 25f;
    [SerializeField] private float stopDistance = 10f;

    [Header("Aim")]
    [SerializeField] private Transform topBone;
    [SerializeField] private float rotateSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private float dampTime = 0.1f;

    private Transform player;
    public Transform Player => player;

    private bool isChasing;

    private Transform[] patrolPoints;
    private int currentPoint;

    private bool isWaiting;
    private float waitTimer;

    private Quaternion startLocalRotation;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        attack = GetComponent<MechaBoBAttack>();

        // Tự xoay bằng script
        agent.updateRotation = false;
    }

    private void Start()
    {
        if (topBone != null)
        {
            startLocalRotation = topBone.localRotation;
        }

        // =========================
        // PATROL ROUTE
        // =========================

        if (patrolRoute == null)
        {
            Debug.LogError(
                $"{name}: Chưa gán Patrol Route!"
            );

            return;
        }

        patrolPoints = new Transform[patrolRoute.childCount];

        for (int i = 0; i < patrolRoute.childCount; i++)
        {
            patrolPoints[i] = patrolRoute.GetChild(i);
        }

        if (patrolPoints.Length == 0)
        {
            Debug.LogError(
                $"{name}: Patrol Route không có Patrol Point!"
            );

            return;
        }

        // =========================
        // NAVMESH
        // =========================

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(
                $"{name}: NavMeshAgent chưa nằm trên NavMesh!"
            );

            return;
        }

        agent.speed = patrolSpeed;

        // Bắt đầu từ Patrol Point đầu tiên
        currentPoint = 0;

        agent.SetDestination(
            patrolPoints[currentPoint].position
        );
    }

    private void Update()
    {
        // Tránh lỗi NavMeshAgent
        if (!agent.isOnNavMesh)
            return;

        // =========================
        // FIND PLAYER
        // =========================

        FindNearestPlayer();

        // =========================
        // CHECK PLAYER
        // =========================

        CheckPlayer();

        // =========================
        // ATTACK
        // =========================

        if (attack != null && attack.IsAttacking)
        {
            agent.isStopped = true;

            Rotate();

            return;
        }

        // =========================
        // CHASE
        // =========================

        if (isChasing)
        {
            ChasePlayer();

            Rotate();

            UpdateAnimation();

            return;
        }

        // =========================
        // PATROL
        // =========================

        Patrol();

        Rotate();

        UpdateAnimation();
    }

    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void FindNearestPlayer()
    {
        GameObject[] players;

        try
        {
            players = GameObject.FindGameObjectsWithTag(playerTag);
        }
        catch
        {
            Debug.LogError(
                $"{name}: Tag '{playerTag}' chưa được tạo!"
            );

            player = null;
            return;
        }

        Transform nearestPlayer = null;

        float nearestDistance = Mathf.Infinity;

        foreach (GameObject playerObject in players)
        {
            if (!playerObject.CompareTag(playerTag))
                continue;

            float distance = Vector3.Distance(
                transform.position,
                playerObject.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlayer = playerObject.transform;
            }
        }

        player = nearestPlayer;
    }

    // =========================================================
    // CHECK PLAYER
    // =========================================================

    private void CheckPlayer()
    {
        if (player == null)
        {
            isChasing = false;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // =========================
        // START CHASE
        // =========================

        if (!isChasing && distance <= chaseRange)
        {
            isChasing = true;

            isWaiting = false;
            waitTimer = 0f;

            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }

        // =========================
        // STOP CHASE
        // =========================

        else if (isChasing && distance >= loseRange)
        {
            isChasing = false;

            agent.isStopped = false;
            agent.speed = patrolSpeed;

            GoToNextPoint();
        }
    }

    // =========================================================
    // PATROL
    // =========================================================

    private void Patrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        // Đã đến Patrol Point
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            // Đang chờ
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = waitTime;
            }

            waitTimer -= Time.deltaTime;

            // Hết thời gian chờ
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                GoToNextPoint();
            }
        }
    }

    // =========================================================
    // NEXT PATROL POINT
    // =========================================================

    private void GoToNextPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        if (randomPatrol)
        {
            currentPoint = Random.Range(
                0,
                patrolPoints.Length
            );
        }
        else
        {
            currentPoint++;

            if (currentPoint >= patrolPoints.Length)
            {
                currentPoint = 0;
            }
        }

        agent.SetDestination(
            patrolPoints[currentPoint].position
        );
    }

    // =========================================================
    // CHASE
    // =========================================================

    private void ChasePlayer()
    {
        if (player == null)
        {
            isChasing = false;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Player vẫn còn trong vùng chase
        if (distance > stopDistance)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;

            if (animator != null &&
                !animator.enabled)
            {
                animator.enabled = true;
            }

            agent.SetDestination(
                player.position
            );
        }
        // Player đã vào khoảng cách bắn
        else
        {
            agent.isStopped = true;

            if (animator != null &&
                animator.enabled)
            {
                animator.enabled = false;
            }

            Rotate();

            if (attack != null)
            {
                attack.Attack();
            }
        }
    }

    // =========================================================
    // ROTATE
    // =========================================================

    private void Rotate()
    {
        Vector3 direction;

        // Khi Chase Player
        if (isChasing && player != null)
        {
            direction =
                player.position -
                transform.position;

            direction.y = 0f;
        }
        // Khi Patrol
        else
        {
            direction = agent.desiredVelocity;
        }

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
    }

    // =========================================================
    // AIM TOP BONE
    // =========================================================

    private void RotateTop()
    {
        if (player == null ||
            topBone == null)
        {
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > stopDistance)
            return;

        Vector3 dir =
            player.position -
            transform.position;

        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        float angle = Vector3.SignedAngle(
            transform.forward,
            dir,
            Vector3.up
        );

        Quaternion target =
            startLocalRotation *
            Quaternion.Euler(
                -angle,
                0f,
                0f
            );

        topBone.localRotation =
            Quaternion.Slerp(
                topBone.localRotation,
                target,
                rotateSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private void UpdateAnimation()
    {
        if (!agent.isOnNavMesh ||
            animator == null)
        {
            return;
        }

        Vector3 localVelocity =
            transform.InverseTransformDirection(
                agent.velocity
            );

        float moveX =
            localVelocity.x /
            Mathf.Max(
                agent.speed,
                0.01f
            );

        float moveY =
            localVelocity.z /
            Mathf.Max(
                agent.speed,
                0.01f
            );

        moveX = Mathf.Clamp(
            moveX,
            -1f,
            1f
        );

        moveY = Mathf.Clamp(
            moveY,
            -1f,
            1f
        );

        if (agent.velocity.magnitude < 0.05f)
        {
            moveX = 0f;
            moveY = 0f;
        }

        animator.SetFloat(
            "MoveX",
            moveX,
            dampTime,
            Time.deltaTime
        );

        animator.SetFloat(
            "MoveY",
            moveY,
            dampTime,
            Time.deltaTime
        );
    }

    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (!agent.isOnNavMesh)
            return;

        RotateTop();
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmos()
    {
        // Chase Range
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            chaseRange
        );

        // Stop Distance
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            stopDistance
        );

        // Lose Range
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            loseRange
        );

        // Patrol Route
        if (patrolRoute == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0;
             i < patrolRoute.childCount;
             i++)
        {
            Transform point =
                patrolRoute.GetChild(i);

            Gizmos.DrawSphere(
                point.position,
                0.2f
            );

            if (i < patrolRoute.childCount - 1)
            {
                Gizmos.DrawLine(
                    point.position,
                    patrolRoute
                        .GetChild(i + 1)
                        .position
                );
            }
        }

        // Nối điểm cuối về điểm đầu
        if (patrolRoute.childCount > 1)
        {
            Gizmos.DrawLine(
                patrolRoute
                    .GetChild(
                        patrolRoute.childCount - 1
                    )
                    .position,

                patrolRoute
                    .GetChild(0)
                    .position
            );
        }

        // Đường ngắm tới Player
        if (topBone != null &&
            player != null)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(
                topBone.position,
                player.position
            );
        }
    }
}
