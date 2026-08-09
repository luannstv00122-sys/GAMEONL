using UnityEngine;
using UnityEngine.UI;

public class LaserUltimateSkill : MonoBehaviour, IPlayerSkillUI
{
    public GameObject laserPrefab;
    public Transform laserPoint;
    public float laserSpeed = 40f;
    public float laserDuration = 3f;
    public float rotationOffsetY = 90f;
    public PlayerMana playerMana;
    public float manaCost = 100f;
    public float cooldown = 20f;
    public Animator animator;
    public string animationTrigger = "UtiShot";
    public Image cooldownImage;
    public Image skillIcon;

    private float currentCooldown;
    private GameObject currentLaser;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerMana == null)
            playerMana = GetComponent<PlayerMana>();

        UpdateCooldownUI();
    }

    private void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            currentCooldown = Mathf.Max(currentCooldown, 0f);
        }

        UpdateCooldownUI();
    }

    public void TryUseFromNetwork(Vector3 aimDirection)
    {
        if (currentCooldown > 0f)
            return;

        if (laserPrefab == null || laserPoint == null)
            return;

        if (playerMana == null || !playerMana.UseMana(manaCost))
            return;

        if (animator != null)
            animator.SetTrigger(animationTrigger);

        if (aimDirection.sqrMagnitude < 0.001f)
            aimDirection = transform.forward;

        Vector3 direction = aimDirection.normalized;

        Quaternion laserRotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(0f, rotationOffsetY, 0f);

        currentLaser = Instantiate(
            laserPrefab,
            laserPoint.position,
            laserRotation
        );

        LaserProjectile projectile =
            currentLaser.GetComponent<LaserProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                laserSpeed,
                laserDuration
            );
        }
        else
        {
            Destroy(currentLaser, laserDuration);
        }

        currentCooldown = cooldown;
        UpdateCooldownUI();
    }

    private void UpdateCooldownUI()
    {
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount =
                cooldown <= 0f
                    ? 1f
                    : 1f - currentCooldown / cooldown;
        }

        if (skillIcon != null)
        {
            skillIcon.color =
                currentCooldown > 0f
                    ? Color.gray
                    : Color.white;
        }
    }

    public void SetPlayerMana(PlayerMana mana)
    {
        playerMana = mana;
    }

    public void SetSkillUI(Image newCooldownImage, Image newSkillIcon)
    {
        cooldownImage = newCooldownImage;
        skillIcon = newSkillIcon;
        UpdateCooldownUI();
    }
}