using UnityEngine;
using UnityEngine.UI;

public class LineFireSkill : MonoBehaviour, IPlayerSkillUI
{
    public GameObject lineFirePrefab;
    public Transform firePoint;
    public PlayerMana playerMana;
    public float manaCost = 50f;
    public float cooldown = 5f;
    public float skillDuration = 10f;
    public Image cooldownImage;
    public Image skillIcon;

    private float currentCooldown;
    private GameObject currentLineFire;

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

        if (lineFirePrefab == null || firePoint == null)
            return;

        if (playerMana == null || !playerMana.UseMana(manaCost))
            return;

        if (currentLineFire != null)
            Destroy(currentLineFire);

        Vector3 flat = aimDirection;
        flat.y = 0f;

        Quaternion rotation =
            flat.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(flat.normalized)
                : firePoint.rotation;

        currentLineFire = Instantiate(
            lineFirePrefab,
            firePoint.position,
            rotation
        );

        Destroy(currentLineFire, skillDuration);

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