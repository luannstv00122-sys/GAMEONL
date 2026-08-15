using UnityEngine;

public class MechaBoBAttack : EnemyAttackBase
{
    [Header("Gun")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] firePoints;

    [Header("Bullet")]
    [SerializeField] private float bulletSpeed = 40f;

    private Transform player;

    private void Start()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");

        if (obj != null)
            player = obj.transform;
    }

    public override void Attack()
{
    if (player == null)
        return;

    if (!CanAttack())
        return;

    StartCoroutine(BurstFire(FireBullets));
}

private void FireBullets()
{
    if (bulletPrefab == null)
        return;

    if (player == null)
        return;

    if (firePoints == null || firePoints.Length == 0)
        return;

    foreach (Transform point in firePoints)
    {
        if (point == null)
            continue;

        GameObject bulletObj = RangedAttack(
            bulletPrefab,
            point,
            player,
            bulletSpeed
        );

        if (bulletObj == null)
            continue;

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            // bullet.damage = bulletDamage;
        }
    }
}
}