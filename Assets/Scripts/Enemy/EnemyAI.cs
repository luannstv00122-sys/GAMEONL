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
    [SerializeField] private Transform player;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float loseRange = 12f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Sight")]
    [SerializeField] private LayerMask sightMask;
    [SerializeField] private Transform eyePoint;

    private EnemyAttackBase enemyAttack;
    private bool isChasing;

    private NavMeshAgent agent;
    private Animator animator;

    private Transform[] patrolPoints;
    private int currentPoint = 0;

    private bool isWaiting;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        enemyAttack = GetComponent<EnemyAttackBase>();

        agent.updateRotation = false;
    }

    private void Start()
    {
        Debug.Log(agent.isOnNavMesh);

        if (patrolRoute == null)
        {
            Debug.LogError("Chưa gán Patrol Route!");
            return;
        }

        patrolPoints = new Transform[patrolRoute.childCount];

        for (int i = 0; i < patrolRoute.childCount; i++)
        {
            patrolPoints[i] = patrolRoute.GetChild(i);
        }

        agent.speed = patrolSpeed;

        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    private void Update()
    {
        CheckPlayer();

        float speed = 0f;

        if (agent.isStopped || agent.velocity.sqrMagnitude < 0.01f)
        {
            speed = 0f;      // Idle
        }
        else if (isChasing)
        {
            speed = 1.5f;      // Run
        }
        else
        {
            speed = 0.5f;    // Walk
        }

        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        if (isChasing)
        {
            ChasePlayer();
            Rotate();
            return;
        }
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
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
        Rotate();
    }

    private void CheckPlayer()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        bool canSee = distance <= chaseRange && CanSeePlayer();

        if (!isChasing && canSee)
        {
            isChasing = true;
            isWaiting = false;
            agent.speed = chaseSpeed;
        }
        else if (isChasing && distance >= loseRange)
        {
            isChasing = false;

            agent.isStopped = false;
            agent.speed = patrolSpeed;

            GoToNextPoint();
        }
    }

    private bool CanSeePlayer()
    {   
        Vector3 origin = eyePoint.position;
        Vector3 direction = (player.position - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, chaseRange, sightMask))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    private void ChasePlayer()
    {
        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > stopDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;

            enemyAttack.Attack();
        }
    }

    private void Rotate()
    {
        Vector3 direction;

        if (isChasing && player != null)
        {
            direction = player.position - transform.position;
            direction.y = 0f;
        }
        else
        {
            direction = agent.desiredVelocity;
        }

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime
        );
    }

    private void GoToNextPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        if (randomPatrol)
        {
            currentPoint = Random.Range(0, patrolPoints.Length);
        }
        else
        {
            currentPoint++;

            if (currentPoint >= patrolPoints.Length)
                currentPoint = 0;
        }

        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    private void OnDrawGizmos()
    {
        // Vùng phát hiện Player
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        // Khoảng cách dừng
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // =========================
        // Patrol Gizmos
        // =========================

        if (patrolRoute == null) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < patrolRoute.childCount; i++)
        {
            Transform point = patrolRoute.GetChild(i);
            Gizmos.DrawSphere(point.position, 0.2f);

            if (i < patrolRoute.childCount - 1)
                Gizmos.DrawLine(point.position, patrolRoute.GetChild(i + 1).position);
        }

        if (patrolRoute.childCount > 1)
            Gizmos.DrawLine(
                patrolRoute.GetChild(patrolRoute.childCount - 1).position,
                patrolRoute.GetChild(0).position);
    }
}