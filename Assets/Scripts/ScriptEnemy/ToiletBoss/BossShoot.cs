using UnityEngine;
using System.Collections;


public class BossShoot : MonoBehaviour
{

    [Header("Gun")]
    [SerializeField] private GameObject bulletPrefab;

    [SerializeField] private Transform firePoint;



    [Header("Bullet")]
    [SerializeField] private float bulletSpeed = 30f;



    [Header("Shoot")]
    [SerializeField] private float attackRange = 15f;

    [SerializeField] private float shootDelay = 0.5f;

    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private float shootAnimationTime = 1.5f;


    [SerializeField] private string shootTrigger = "Shoot";



    public float AttackRange => attackRange;



    private Animator animator;


    private float cooldownTimer;



    public bool IsAttacking { get; private set; }




    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }




    private void Update()
    {

        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

    }





    public void Attack()
    {

        if (IsAttacking)
            return;


        if (cooldownTimer > 0)
            return;



        IsAttacking = true;


        cooldownTimer = shootCooldown;



        animator.SetTrigger(shootTrigger);



        StartCoroutine(ShootDelay());

    }


    private IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(shootDelay);

        Shoot();

        yield return new WaitForSeconds(shootAnimationTime);

        IsAttacking = false;
    }


    private void Shoot()
    {

        if (bulletPrefab == null ||
           firePoint == null)
            return;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");


        if (player == null)
            return;



        Vector3 direction =
            (player.transform.position -
             firePoint.position).normalized;



        GameObject bullet =
            Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.LookRotation(direction));

        Rigidbody rb =
            bullet.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.linearVelocity =
                direction * bulletSpeed;
        }

    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;


        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);

    }

}