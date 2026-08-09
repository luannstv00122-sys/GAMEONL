using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Melee")]
    [SerializeField] private float attackRange = 3f;

    public float AttackRange => attackRange;


    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;


    [Header("Animator")]
    [SerializeField] private string attackTrigger = "Attack";


    private Animator animator;


    private float cooldownTimer;


    public bool IsAttacking { get; private set; }



    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }



    private void Update()
    {
        if(cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }




    public void Attack()
    {
         Debug.Log("Start Attack");

        if(IsAttacking)
            return;


        if(cooldownTimer > 0)
            return;



        IsAttacking = true;


        cooldownTimer = attackCooldown;



        animator.SetTrigger(attackTrigger);

    }




    // Animation Event gọi hàm này
    public void FinishAttack()
    {
        Debug.Log("Finish Attack");

        IsAttacking = false;
    }




    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }

}