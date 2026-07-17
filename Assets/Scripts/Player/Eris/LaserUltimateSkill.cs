using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LaserUltimateSkill : MonoBehaviour
{
    [Header("Laser Skill")]
    public GameObject laserPrefab;
    public Transform laserPoint;
    public Camera playerCamera;

    [Header("Laser Movement")]
    public float laserSpeed = 40f;
    public float laserDuration = 3f;
    public float rotationOffsetY = 90f;

    [Header("Mana")]
    public PlayerMana playerMana;
    public float manaCost = 100f;

    [Header("Cooldown")]
    public float cooldown = 20f;
    private float currentCooldown;

    [Header("Animation")]
    public Animator animator;
    public string animationTrigger = "UtiShot";

    [Header("UI")]
    public Image cooldownImage;
    public Image skillIcon;

    private GameObject currentLaser;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerMana == null)
            playerMana = GetComponent<PlayerMana>();

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f;
    }

    void Update()
    {
        UpdateCooldown();

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            UseLaserSkill();
        }
    }

    void UseLaserSkill()
    {
        if (currentCooldown > 0f)
        {
            Debug.Log("Chiêu laser đang hồi");
            return;
        }

        if (laserPrefab == null || laserPoint == null || playerCamera == null)
        {
            Debug.LogWarning(
                "Thiếu Laser Prefab, Laser Point hoặc Player Camera"
            );
            return;
        }

        if (playerMana == null || !playerMana.UseMana(manaCost))
        {
            Debug.Log("Không đủ mana để dùng laser");
            return;
        }

        if (animator != null)
            animator.SetTrigger(animationTrigger);

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 direction = ray.direction.normalized;

        Quaternion laserRotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(0f, rotationOffsetY, 0f);

        currentLaser = Instantiate(
            laserPrefab,
            laserPoint.position,
            laserRotation
        );

        LaserProjectile laserProjectile =
            currentLaser.GetComponent<LaserProjectile>();

        if (laserProjectile != null)
        {
            laserProjectile.Initialize(
                direction,
                laserSpeed,
                laserDuration
            );
        }
        else
        {
            Debug.LogWarning(
                "Prefab Laser chưa gắn script LaserProjectile"
            );

            Destroy(currentLaser, laserDuration);
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

            if (cooldownImage != null)
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