using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BlackHoleSkill : MonoBehaviour
{
    [Header("Black Hole")]
    public GameObject blackHolePrefab;
    public Transform shootPoint;
    public Camera playerCamera;

    [Header("Projectile")]
    public float projectileSpeed = 20f;
    public float projectileLifeTime = 3f;

    [Header("Mana")]
    public PlayerMana playerMana;
    public float manaCost = 50f;

    [Header("Cooldown")]
    public float cooldown = 10f;
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
            CastBlackHole();
        }
    }

    void CastBlackHole()
    {
        if (currentCooldown > 0f)
        {
            Debug.Log("Skill hố đen đang hồi chiêu");
            return;
        }

        if (blackHolePrefab == null ||
            shootPoint == null ||
            playerCamera == null)
        {
            Debug.LogWarning(
                "Thiếu Black Hole Prefab, Shoot Point hoặc Player Camera"
            );
            return;
        }

        if (playerMana == null || !playerMana.UseMana(manaCost))
        {
            Debug.Log("Không đủ mana để dùng hố đen");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 direction = ray.direction.normalized;

        GameObject blackHole = Instantiate(
            blackHolePrefab,
            shootPoint.position,
            Quaternion.LookRotation(direction)
        );

        BlackHoleProjectile projectile =
            blackHole.GetComponent<BlackHoleProjectile>();

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
                "Prefab hố đen chưa gắn BlackHoleProjectile"
            );

            Destroy(blackHole, projectileLifeTime);
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
            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f;

            if (skillIcon != null)
                skillIcon.color = Color.white;
        }
    }
}