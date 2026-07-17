using UnityEngine;
using UnityEngine.InputSystem;

public class GunFireFX : MonoBehaviour
{
    public GameObject firePrefab;
    public Transform firePoint;
    public Camera playerCamera;

    public float projectileSpeed = 25f;
    public float lifeTime = 3f;
    public float fireScale = 0.4f;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (firePrefab == null || firePoint == null || playerCamera == null)
        {
            Debug.LogWarning("Thiếu Fire Prefab, Fire Point hoặc Player Camera");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 direction = ray.direction;

        GameObject fire = Instantiate(
            firePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        FireProjectile projectile = fire.GetComponent<FireProjectile>();

        fire.transform.localScale = Vector3.one * fireScale;

        if (FireBulletBuffSkill.IsBuffActive)
        {
            fire.transform.localScale = Vector3.one * fireScale * FireBulletBuffSkill.BulletScaleMultiplier;

            if (projectile != null)
            {
                projectile.damage *= FireBulletBuffSkill.DamageMultiplier;
            }

            Debug.Log("Đạn buff ON - Scale: " + fire.transform.localScale);
        }

        if (projectile != null)
        {
            projectile.SetDirection(direction, projectileSpeed, lifeTime);
        }
    }
}