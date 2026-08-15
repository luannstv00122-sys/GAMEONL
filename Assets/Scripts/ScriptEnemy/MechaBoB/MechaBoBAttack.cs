using UnityEngine;

public class MechaBoBAttack : EnemyAttackBase
{
    [Header("Gun")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] firePoints;

    [Header("Bullet")]
    [SerializeField] private float bulletSpeed = 40f;

    private MechaBoBMovement movement;

    private void Awake()
    {
        movement = GetComponent<MechaBoBMovement>();
    }

    public override void Attack()
    {
        if (!CanAttack())
            return;

        if (movement == null)
            return;

        if (movement.Player == null)
            return;

        StartCoroutine(BurstFire(FireBullets));
    }

    private void FireBullets()
    {
        if (bulletPrefab == null)
            return;

        if (movement == null)
            return;

        if (movement.Player == null)
            return;

        foreach (Transform point in firePoints)
        {
            if (point == null)
                continue;

            RangedAttack(
                bulletPrefab,
                point,
                movement.Player,
                bulletSpeed
            );
        }
    }
}