using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FireBulletBuffSkill : MonoBehaviour
{
    public static bool IsBuffActive = false;
    public static float DamageMultiplier = 1f;
    public static float BulletScaleMultiplier = 1f;

    [Header("Mana")]
    public PlayerMana playerMana;
    public float manaCost = 100f;

    [Header("Cooldown")]
    public float cooldown = 20f;
    private float currentCooldown = 0f;

    [Header("Buff Duration")]
    public float buffDuration = 10f;
    private float currentBuffTime = 0f;

    [Header("Buff Settings")]
    public float damageMultiplier = 2f;
    public float bulletScaleMultiplier = 5f;

    [Header("UI")]
    public Image cooldownImage;
    public Image skillIcon;

    void Start()
    {
        IsBuffActive = false;
        DamageMultiplier = 1f;
        BulletScaleMultiplier = 1f;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f;
    }

    void Update()
    {
        UpdateCooldown();
        UpdateBuffTime();

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ActivateBuff();
        }
    }

    void ActivateBuff()
    {
        if (currentCooldown > 0)
            return;

        if (playerMana == null || !playerMana.UseMana(manaCost))
        {
            Debug.Log("Không đủ Mana để dùng Skill E");
            return;
        }

        IsBuffActive = true;
        DamageMultiplier = damageMultiplier;
        BulletScaleMultiplier = bulletScaleMultiplier;

        currentBuffTime = buffDuration;
        currentCooldown = cooldown;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        Debug.Log("Skill E: Đạn lửa đã được cường hóa");
    }

    void UpdateBuffTime()
    {
        if (!IsBuffActive) return;

        currentBuffTime -= Time.deltaTime;

        if (currentBuffTime <= 0)
        {
            IsBuffActive = false;
            DamageMultiplier = 1f;
            BulletScaleMultiplier = 1f;
            currentBuffTime = 0f;

            Debug.Log("Skill E: Hết cường hóa đạn lửa");
        }
    }

    void UpdateCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;

            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f - (currentCooldown / cooldown);

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