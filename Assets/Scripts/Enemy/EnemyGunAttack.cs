using UnityEngine;

public class EnemyGunAttack : EnemyAttackBase
{
    [Header("Gun")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public float bulletDamage = 20f;
    public float bulletSpeed = 30f;

    public override void Attack()
    {
        if (!CanAttack())
            return;

        animator.SetTrigger("Shoot");

        cooldownTimer = attackCooldown;
    }

    public void FireBullet()
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation);

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        bulletScript.damage = bulletDamage;
        bulletScript.speed = bulletSpeed;
    }
}