using System;
using System.Collections;
using UnityEngine;

public abstract class EnemyAttackBase : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] protected float attackCooldown = 1.5f;

    [Header("Burst Fire")]
    [SerializeField] protected bool useBurst = false;
    [SerializeField] protected int burstCount = 3;
    [SerializeField] protected float burstInterval = 0.15f;

    [Header("Shoot Timing")]
    [SerializeField] protected float fireDelay = 0.3f;

    protected float cooldownTimer;
    protected Animator animator;

    protected bool isAttacking;
    public bool IsAttacking => isAttacking;

    protected EnemyAI enemyAI;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyAI>();
    }

    protected virtual void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public bool CanAttack()
    {
        return cooldownTimer <= 0f && !isAttacking;
    }

    protected virtual Collider[] MeleeAttack(Vector3 boxSize, float forwardOffset, LayerMask targetLayer)
    {
        Vector3 center = transform.position + transform.forward * forwardOffset;

        return Physics.OverlapBox(
            center,
            boxSize * 0.5f,
            transform.rotation,
            targetLayer
        );
    }

    protected virtual GameObject RangedAttack(
    GameObject bulletPrefab,
    Transform firePoint,
    Transform target,
    float bulletSpeed)
{
    if (bulletPrefab == null)
    {
        Debug.LogError($"{name}: Bullet Prefab đang NULL!");
        return null;
    }

    if (firePoint == null)
    {
        Debug.LogError($"{name}: Fire Point đang NULL!");
        return null;
    }

    if (target == null)
    {
        Debug.LogWarning($"{name}: Target đã NULL, không thể bắn!");
        return null;
    }

    Collider col = target.GetComponent<Collider>();

    Vector3 targetPoint = target.position;

    if (col != null)
        targetPoint = col.bounds.center;

    Vector3 direction = (targetPoint - firePoint.position).normalized;

    if (direction.sqrMagnitude <= 0.001f)
        return null;

    Quaternion rotation = Quaternion.LookRotation(direction);

    GameObject bullet = Instantiate(
        bulletPrefab,
        firePoint.position,
        rotation
    );

    Debug.Log(
        $"💥 SPAWN BULLET | ID: {bullet.GetInstanceID()} | Enemy: {name}"
    );

    Rigidbody rb = bullet.GetComponent<Rigidbody>();

    if (rb != null)
        rb.linearVelocity = direction * bulletSpeed;

    Bullet bulletScript = bullet.GetComponent<Bullet>();

    if (bulletScript != null)
        bulletScript.speed = bulletSpeed;

    return bullet;
}

    protected IEnumerator BurstFire(Action fireAction)
    {
        isAttacking = true;

        // Chờ đến thời điểm bắn
        if (fireDelay > 0f)
            yield return new WaitForSeconds(fireDelay);

        int count = useBurst ? burstCount : 1;

        for (int i = 0; i < count; i++)
        {
            fireAction?.Invoke();

            if (i < count - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        cooldownTimer = attackCooldown;
        isAttacking = false;
    }

    public abstract void Attack();
}