using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FireBallSkill : MonoBehaviour
{
    [Header("Fire Ball")]
    public GameObject fireBallPrefab;
    public Transform firePoint;
    public Camera playerCamera;

    [Header("Projectile")]
    public float projectileSpeed = 20f;
    public float projectileLifeTime = 5f;

    [Header("Mana")]
    public PlayerMana playerMana;
    public float manaCost = 50f;

    [Header("Cooldown")]
    public float cooldown = 8f;
    private float currentCooldown;

    [Header("UI")]
    public Image cooldownImage;
    public Image skillIcon;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerMana == null)
            playerMana = GetComponent<PlayerMana>();

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f;
    }

    void Update()
    {
        UpdateCooldown();

        if (Keyboard.current != null &&
            Keyboard.current.qKey.wasPressedThisFrame)
        {
            CastFireBall();
        }
    }

    void CastFireBall()
    {
        if (currentCooldown > 0f)
        {
            Debug.Log("Skill quả cầu lửa đang hồi");
            return;
        }

        if (fireBallPrefab == null ||
            firePoint == null ||
            playerCamera == null)
        {
            Debug.LogWarning(
                "Thiếu Fire Ball Prefab, Fire Point hoặc Player Camera"
            );
            return;
        }

        if (playerMana == null || !playerMana.UseMana(manaCost))
        {
            Debug.Log("Không đủ mana để dùng quả cầu lửa");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 direction = ray.direction.normalized;

        GameObject fireBall = Instantiate(
            fireBallPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        FireBallProjectile projectile =
            fireBall.GetComponent<FireBallProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                projectileSpeed,
                projectileLifeTime
            );
        }
        else
        {
            Debug.LogWarning(
                "Prefab quả cầu lửa chưa gắn FireBallProjectile"
            );

            Destroy(fireBall, projectileLifeTime);
        }

        currentCooldown = cooldown;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;
    }

    void UpdateCooldown()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            currentCooldown = Mathf.Max(currentCooldown, 0f);

            if (cooldownImage != null && cooldown > 0f)
            {
                cooldownImage.fillAmount =
                    1f - currentCooldown / cooldown;
            }

            if (skillIcon != null)
                skillIcon.color = Color.gray;
        }
        else
        {
            currentCooldown = 0f;

            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f;

            if (skillIcon != null)
                skillIcon.color = Color.white;
        }
    }
}