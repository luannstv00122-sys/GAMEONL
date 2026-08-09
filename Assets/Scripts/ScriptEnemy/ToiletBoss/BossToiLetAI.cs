using UnityEngine;
using UnityEngine.AI;

public class BossToiLetAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;


    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float loseRange = 25f;


    [Header("Attack")]
    [SerializeField] private BossAttack meleeAttack;
    [SerializeField] private BossShoot rangedAttack;


    private NavMeshAgent agent;
    private Animator animator;


    private Transform player;

    private bool isChasing;


    private float findPlayerTimer;

    [SerializeField]
    private float findPlayerInterval = 0.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();


        agent.updateRotation = false;



        if (meleeAttack == null)
            meleeAttack = GetComponent<BossAttack>();


        if (rangedAttack == null)
            rangedAttack = GetComponent<BossShoot>();
    }




    private void Start()
    {
        agent.speed = moveSpeed;
    }

    private void Update()
    {

        FindPlayerTimer();


        if (player == null)
            return;

        CheckPlayer();

        // Nếu đang attack
        if (isChasing)
        {
            ChasePlayer();

            Rotate();
        }


        // Chỉ update chạy khi không attack
        if (!IsAttacking())
        {
            UpdateAnimation();
        }

    }

    private void FindPlayerTimer()
    {
        findPlayerTimer -= Time.deltaTime;


        if (findPlayerTimer <= 0)
        {
            FindPlayer();

            findPlayerTimer = findPlayerInterval;
        }
    }




    private void FindPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        Transform nearestPlayer = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject obj in players)
        {
            if (!obj.CompareTag("Player"))
                continue;

            float distance = Vector3.Distance(
                transform.position,
                obj.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlayer = obj.transform;
            }
        }

        player = nearestPlayer;
    }




    private void CheckPlayer()
    {
        float distance =
            Vector3.Distance(
                transform.position,
                player.position);



        if (!isChasing &&
           distance <= detectionRange)
        {
            isChasing = true;
        }



        else if (isChasing &&
                distance >= loseRange)
        {
            isChasing = false;


            agent.isStopped = false;
        }

    }


    private void ChasePlayer()
    {
        // Nếu đang đánh thì đứng yên
        if (IsAttacking())
        {
            agent.isStopped = true;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position);

        if (CanAttack(distance))
        {
            agent.isStopped = true;
            TryAttack();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    private void TryAttack()
    {
        if (IsAttacking())
            return;



        float distance =
            Vector3.Distance(
                transform.position,
                player.position);



        // Ưu tiên melee
        if (meleeAttack != null &&
           distance <= meleeAttack.AttackRange)
        {
            meleeAttack.Attack();

            return;
        }



        // Bắn xa
        if (rangedAttack != null &&
           distance <= rangedAttack.AttackRange)
        {
            rangedAttack.Attack();
        }
    }

    private bool CanAttack(float distance)
    {
        bool melee =
            meleeAttack != null &&
            distance <= meleeAttack.AttackRange;



        bool shoot =
            rangedAttack != null &&
            distance <= rangedAttack.AttackRange;



        return melee || shoot;
    }

    private bool IsAttacking()
    {

        bool melee =
            meleeAttack != null &&
            meleeAttack.IsAttacking;



        bool shoot =
            rangedAttack != null &&
            rangedAttack.IsAttacking;



        return melee || shoot;

    }

    private void Rotate()
    {

        if (player == null)
            return;



        Vector3 direction =
            player.position -
            transform.position;



        direction.y = 0;



        if (direction.sqrMagnitude < 0.01f)
            return;



        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized);



        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime);

    }


    private void UpdateAnimation()
    {
        if (animator == null)
            return;


        float speed = 0;


        if (!agent.isStopped &&
            agent.velocity.sqrMagnitude > 0.01f)
        {
            speed = 1.5f;
        }


        animator.SetFloat(
            "Speed",
            speed,
            0.1f,
            Time.deltaTime);
    }
}