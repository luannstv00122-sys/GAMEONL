using UnityEngine;
using UnityEngine.UI;

public class SmokeSkill : MonoBehaviour, IPlayerSkillUI
{
    public GameObject smokePrefab;
    public Transform smokePoint;
    public float smokeSpeed = 15f;
    public float smokeLifeTime = 2f;
    public float spawnRate = 0.05f;
    public float skillDuration = 10f;
    public PlayerMana playerMana;
    public float manaCost = 50f;
    public float cooldown = 20f;
    public Image cooldownImage;
    public Image skillIcon;

    private float currentCooldown;
    private bool isUsingSkill;
    private float skillTimer;
    private float spawnTimer;
    private Vector3 networkAimDirection;

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

        if (isUsingSkill)
            UpdateSkill();
    }

    public void TryUseFromNetwork(Vector3 aimDirection)
    {
        if (currentCooldown > 0f)
            return;

        if (smokePrefab == null || smokePoint == null)
            return;

        if (playerMana == null || !playerMana.UseMana(manaCost))
            return;

        networkAimDirection =
            aimDirection.sqrMagnitude > 0.001f
                ? aimDirection.normalized
                : transform.forward;

        currentCooldown = cooldown;
        skillTimer = skillDuration;
        spawnTimer = 0f;
        isUsingSkill = true;

        UpdateCooldownUI();
    }

    private void UpdateSkill()
    {
        skillTimer -= Time.deltaTime;

        if (skillTimer <= 0f)
        {
            isUsingSkill = false;
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnRate)
            return;

        spawnTimer = 0f;

        Vector3 direction = networkAimDirection;

        GameObject smoke = Instantiate(
            smokePrefab,
            smokePoint.position,
            Quaternion.LookRotation(direction)
        );

        SmokeProjectile projectile =
            smoke.GetComponent<SmokeProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                smokeSpeed,
                smokeLifeTime
            );
        }
        else
        {
            Destroy(smoke, smokeLifeTime);
        }
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