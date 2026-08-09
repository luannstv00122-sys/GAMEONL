using UnityEngine;

public class EnemyGunAttack : EnemyAttackBase
{
    [Header("Gun")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Bullet")]
    [SerializeField] private float bulletDamage = 20f;
    [SerializeField] private float bulletSpeed = 30f;

    public override void Attack()
    {
        if (!CanAttack())
            return;

        animator.SetTrigger("Shoot");

        StartCoroutine(BurstFire(FireBullet));
    }

    private void FireBullet()
    {
        if (enemyAI == null || enemyAI.Player == null)
            return;

        GameObject bulletObj = RangedAttack(
            bulletPrefab,
            firePoint,
            enemyAI.Player,
            bulletSpeed
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            // bullet.damage = bulletDamage;
        }
    }
}