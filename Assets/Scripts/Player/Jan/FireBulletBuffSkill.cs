using UnityEngine;
using UnityEngine.UI;

public class FireBulletBuffSkill : MonoBehaviour, IPlayerSkillUI
{
    public PlayerMana playerMana;
    public float manaCost = 100f;
    public float cooldown = 20f;
    public float buffDuration = 10f;
    public float damageMultiplier = 2f;
    public float bulletScaleMultiplier = 5f;
    public Image cooldownImage;
    public Image skillIcon;

    private float currentCooldown;
    private float currentBuffTime;

    public bool IsBuffActive { get; private set; }
    public float DamageMultiplier { get; private set; } = 1f;
    public float BulletScaleMultiplier { get; private set; } = 1f;

    private void Start()
    {
        if (playerMana == null)
            playerMana = GetComponent<PlayerMana>();

        ResetBuff();
        UpdateCooldownUI();
    }

    private void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            currentCooldown = Mathf.Max(currentCooldown, 0f);
        }

        if (IsBuffActive)
        {
            currentBuffTime -= Time.deltaTime;

            if (currentBuffTime <= 0f)
                ResetBuff();
        }

        UpdateCooldownUI();
    }

    public void TryUseFromNetwork(Vector3 unusedAimDirection)
    {
        if (currentCooldown > 0f)
            return;

        if (playerMana == null || !playerMana.UseMana(manaCost))
            return;

        IsBuffActive = true;
        DamageMultiplier = damageMultiplier;
        BulletScaleMultiplier = bulletScaleMultiplier;

        currentBuffTime = buffDuration;
        currentCooldown = cooldown;

        UpdateCooldownUI();
    }

    private void ResetBuff()
    {
        IsBuffActive = false;
        DamageMultiplier = 1f;
        BulletScaleMultiplier = 1f;
        currentBuffTime = 0f;
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

    public void SetSkillUI(Image cooldown, Image icon)
    {
        cooldownImage = cooldown;
        skillIcon = icon;
        UpdateCooldownUI();
    }

    public void SetPlayerMana(PlayerMana mana)
    {
        playerMana = mana;
    }
}