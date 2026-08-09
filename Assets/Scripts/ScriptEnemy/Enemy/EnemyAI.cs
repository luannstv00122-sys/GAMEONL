using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform patrolRoute;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private bool randomPatrol = false;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Chase")]
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float loseRange = 12f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Sight")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private string playerTag = "Player";

    private EnemyAttackBase enemyAttack;

    private bool isChasing;
    private Transform player;

    private NavMeshAgent agent;
    private Animator animator;

    private Transform[] patrolPoints;
    private int currentPoint = 0;

    private bool isWaiting;
    private float waitTimer;

    public Transform Player => player;

    private float findPlayerTimer;

    [SerializeField]
    private float findPlayerInterval = 0.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        enemyAttack = GetComponent<EnemyAttackBase>();

        agent.updateRotation = false;
    }

    private void Start()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError(
                $"{name}: NavMeshAgent chưa nằm trên NavMesh!"
            );

            return;
        }

        if (patrolRoute == null)
        {
            Debug.LogError(
                $"{name}: Chưa gán Patrol Route!"
            );

            return;
        }

        patrolPoints =
            new Transform[patrolRoute.childCount];

        for (int i = 0;
             i < patrolRoute.childCount;
             i++)
        {
            patrolPoints[i] =
                patrolRoute.GetChild(i);
        }

        if (patrolPoints.Length == 0)
        {
            Debug.LogError(
                $"{name}: Patrol Route không có Patrol Point!"
            );

            return;
        }

        agent.speed = patrolSpeed;

        agent.SetDestination(
            patrolPoints[currentPoint].position
        );
    }

    private void Update()
    {
        // Tránh lỗi NavMeshAgent
        if (!agent.isOnNavMesh)
            return;

        // ==================================================
        // Tìm Player gần nhất bằng Tag
        // ==================================================

        findPlayerTimer -= Time.deltaTime;

        if (findPlayerTimer <= 0f)
        {
            FindNearestPlayer();

            findPlayerTimer =
                findPlayerInterval;
        }

        CheckPlayer();

        // ==================================================
        // Đang attack
        // ==================================================

        if (enemyAttack != null &&
            enemyAttack.IsAttacking)
        {
            agent.isStopped = true;

            Rotate();

            return;
        }

        // ==================================================
        // Animation
        // ==================================================

        float speed = 0f;

        if (agent.isStopped ||
            agent.velocity.sqrMagnitude < 0.01f)
        {
            speed = 0f;
        }
        else if (isChasing)
        {
            speed = 1.5f;
        }
        else
        {
            speed = 0.5f;
        }

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                speed,
                0.1f,
                Time.deltaTime
            );
        }

        // ==================================================
        // Chase
        // ==================================================

        if (isChasing)
        {
            ChasePlayer();

            Rotate();

            return;
        }

        // ==================================================
        // Patrol Waiting
        // ==================================================

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;

                GoToNextPoint();
            }

            return;
        }

        // ==================================================
        // Đến Patrol Point
        // ==================================================

        if (!agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance)
        {
            isWaiting = true;

            waitTimer = waitTime;
        }

        Rotate();
    }

    // =========================================================
    // FIND NEAREST PLAYER
    // =========================================================

    /// <summary>
    /// Tìm Player gần nhất bằng Tag "Player".
    /// Hỗ trợ nhiều Player trong Multiplayer.
    /// </summary>
    private void FindNearestPlayer()
    {
        GameObject[] players;

        try
        {
            players =
                GameObject.FindGameObjectsWithTag(
                    playerTag
                );
        }
        catch
        {
            Debug.LogError(
                $"{name}: Tag '{playerTag}' chưa được tạo!"
            );

            player = null;

            return;
        }

        float nearestDistance =
            Mathf.Infinity;

        Transform nearestPlayer = null;

        foreach (GameObject obj in players)
        {
            // Kiểm tra bằng CompareTag
            if (!obj.CompareTag(playerTag))
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    obj.transform.position
                );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;

                nearestPlayer =
                    obj.transform;
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

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // Player trong Chase Range
        bool canSee =
            distance <= chaseRange &&
            CanSeePlayer();

        // Bắt đầu Chase
        if (!isChasing && canSee)
        {
            isChasing = true;

            isWaiting = false;

            agent.speed = chaseSpeed;
        }

        // Player chạy quá xa
        else if (isChasing &&
                 distance >= loseRange)
        {
            isChasing = false;

            agent.isStopped = false;

            agent.speed = patrolSpeed;

            GoToNextPoint();
        }
    }

    // =========================================================
    // CHECK SIGHT BY TAG
    // =========================================================

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        if (eyePoint == null)
            return false;

        Vector3 origin =
            eyePoint.position;

        Vector3 direction =
            (
                player.position -
                origin
            ).normalized;

        // Không sử dụng LayerMask nữa
        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            chaseRange))
        {
            // Player được nhận diện bằng Tag
            return hit.collider.CompareTag(
                playerTag
            );
        }

        return false;
    }

    // =========================================================
    // CHASE PLAYER
    // =========================================================

    private void ChasePlayer()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance > stopDistance)
        {
            agent.isStopped = false;

            agent.speed = chaseSpeed;

            agent.SetDestination(
                player.position
            );
        }
        else
        {
            agent.isStopped = true;

            if (enemyAttack != null)
            {
                enemyAttack.Attack();
            }
        }
    }

    // =========================================================
    // ROTATE
    // =========================================================

    private void Rotate()
    {
        Vector3 direction;

        if (isChasing &&
            player != null)
        {
            direction =
                player.position -
                transform.position;

            direction.y = 0f;
        }
        else
        {
            direction =
                agent.desiredVelocity;
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
            currentPoint =
                Random.Range(
                    0,
                    patrolPoints.Length
                );
        }
        else
        {
            currentPoint++;

            if (currentPoint >=
                patrolPoints.Length)
            {
                currentPoint = 0;
            }
        }

        agent.SetDestination(
            patrolPoints[currentPoint].position
        );
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

            if (i <
                patrolRoute.childCount - 1)
            {
                Gizmos.DrawLine(
                    point.position,
                    patrolRoute
                        .GetChild(i + 1)
                        .position
                );
            }
        }

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

        // Eye → Player
        if (eyePoint != null &&
            player != null)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(
                eyePoint.position,
                player.position
            );
        }
    }
}
