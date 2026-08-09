using UnityEngine;

public class SimpleGun : MonoBehaviour
{
    [Header("Fire")]
    public GameObject firePrefab;
    public Transform firePoint;

    [Header("Projectile")]
    public float projectileSpeed = 25f;
    public float lifeTime = 3f;
    public float fireScale = 0.4f;

    [Header("Buff")]
    [SerializeField] private FireBulletBuffSkill buffSkill;

    private void Awake()
    {
        if (buffSkill == null)
            buffSkill = GetComponent<FireBulletBuffSkill>();

        if (buffSkill == null)
            buffSkill = GetComponentInParent<FireBulletBuffSkill>();

        if (buffSkill == null)
            buffSkill = GetComponentInChildren<FireBulletBuffSkill>(true);
    }

    // Không đọc chuột trực tiếp ở đây.
    // NetworkInvectorController gọi hàm này cho đúng player.
    public void ShootFromNetwork(Vector3 aimDirection)
    {
        if (firePrefab == null || firePoint == null)
        {
            Debug.LogWarning(
                $"{name}: Thiếu Fire Prefab hoặc Fire Point."
            );
            return;
        }

        if (aimDirection.sqrMagnitude < 0.001f)
            aimDirection = transform.forward;

        Vector3 direction = aimDirection.normalized;

        GameObject fire = Instantiate(
            firePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        FireProjectile projectile =
            fire.GetComponent<FireProjectile>();

        float finalScale = fireScale;

        if (buffSkill != null && buffSkill.IsBuffActive)
        {
            finalScale *= buffSkill.BulletScaleMultiplier;

            if (projectile != null)
                projectile.damage *= buffSkill.DamageMultiplier;
        }

        fire.transform.localScale =
            Vector3.one * finalScale;

        if (projectile != null)
        {
            projectile.SetDirection(
                direction,
                projectileSpeed,
                lifeTime
            );
        }
        else
        {
            Destroy(fire, lifeTime);
        }
    }
}