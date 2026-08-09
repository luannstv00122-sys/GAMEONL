using UnityEngine;
using UnityEngine.UI;

public class BlackHoleSkill : MonoBehaviour, IPlayerSkillUI
{
    public GameObject blackHolePrefab;
    public Transform shootPoint;
    public float projectileSpeed = 20f;
    public float projectileLifeTime = 3f;
    public PlayerMana playerMana;
    public float manaCost = 50f;
    public float cooldown = 10f;
    public Image cooldownImage;
    public Image skillIcon;

    private float currentCooldown;

    private void Start()
    {
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

        if (blackHolePrefab == null || shootPoint == null)
            return;

        if (playerMana == null || !playerMana.UseMana(manaCost))
            return;

        if (aimDirection.sqrMagnitude < 0.001f)
            aimDirection = transform.forward;

        Vector3 direction = aimDirection.normalized;

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
            Destroy(blackHole, projectileLifeTime);
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