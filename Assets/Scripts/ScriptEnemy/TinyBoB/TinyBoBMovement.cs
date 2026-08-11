using UnityEngine;
using UnityEngine.AI;

public class TinyBoBMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Patrol")]
    [SerializeField] private Transform patrolRoute;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private bool randomPatrol = false;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 8f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Chase")]
    [SerializeField] private float chaseRange = 20f;
    [SerializeField] private float loseRange = 25f;

    [Header("Animation")]
    [SerializeField] private float dampTime = 0.1f;

    private Transform player;

    private bool isChasing;

    private Transform[] patrolPoints;
    private int currentPoint;

    private bool isWaiting;
    private float waitTimer;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // Tự xoay bằng script
        agent.updateRotation = false;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
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

        // TinyBoB không cần dừng để đánh
        agent.stoppingDistance = 0f;

        // Bắt đầu từ Patrol Point đầu tiên
        currentPoint = 0;

        agent.SetDestination(
            patrolPoints[currentPoint].position
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

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
    // CHASE PLAYER
    // =========================================================

    private void ChasePlayer()
    {
        if (player == null)
        {
            isChasing = false;
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        if (animator != null &&
            !animator.enabled)
        {
            animator.enabled = true;
        }

        // Luôn chạy tới Player
        agent.SetDestination(
            player.position
        );
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
    // COLLISION - CHẠM PLAYER
    // =========================================================

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            Destroy(gameObject);
        }
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
    }
}
