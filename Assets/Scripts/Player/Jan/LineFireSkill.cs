using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LineFireSkill : MonoBehaviour
{
    [Header("Skill")]
    public GameObject lineFirePrefab;
    public Transform firePoint;

    [Header("Mana")]
    public PlayerMana playerMana;
    public float manaCost = 50f;

    [Header("Cooldown")]
    public float cooldown = 5f;
    private float currentCooldown = 0f;

    [Header("Skill Duration")]
    public float skillDuration = 10f;

    [Header("UI")]
    public Image cooldownImage;      // Image Radial 360
    public Image skillIcon;          // Icon Skill

    private GameObject currentLineFire;

    void Start()
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount = 0;
    }

    void Update()
    {
        UpdateCooldown();

        if (Keyboard.current != null &&
            Keyboard.current.qKey.wasPressedThisFrame)
        {
            CastSkill();
        }
    }

    void CastSkill()
    {
        // Đang hồi chiêu
        if (currentCooldown > 0)
            return;

        // Không đủ mana
        if (playerMana == null || !playerMana.UseMana(manaCost))
        {
            Debug.Log("Không đủ Mana!");
            return;
        }

        // Nếu còn Line Fire cũ thì xóa
        if (currentLineFire != null)
        {
            Destroy(currentLineFire);
        }

        // Tạo Line Fire
        currentLineFire = Instantiate(
            lineFirePrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Sau 10 giây tự hủy
        Destroy(currentLineFire, skillDuration);

        // Bắt đầu hồi chiêu
        currentCooldown = cooldown;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;
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
            currentCooldown = 0;

            if (cooldownImage != null)
                cooldownImage.fillAmount = 1;

            if (skillIcon != null)
                skillIcon.color = Color.white;
        }
    }
}