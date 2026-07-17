using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SmokeSkill : MonoBehaviour
{
    [Header("Smoke")]
    public GameObject smokePrefab;
    public Transform smokePoint;
    public Camera playerCamera;

    [Header("Smoke Settings")]
    public float smokeSpeed = 15f;
    public float smokeLifeTime = 2f;
    public float spawnRate = 0.05f;
    public float skillDuration = 10f;

    [Header("Mana")]
    public PlayerMana playerMana;
    public float manaCost = 50;

    [Header("Cooldown")]
    public float cooldown = 20f;

    private float currentCooldown;

    [Header("UI")]
    public Image cooldownImage;
    public Image skillIcon;

    bool isUsingSkill;

    float skillTimer;

    float spawnTimer;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerMana == null)
            playerMana = GetComponent<PlayerMana>();

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1;
    }

    void Update()
    {
        UpdateCooldown();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartSkill();
        }

        if (isUsingSkill)
        {
            UpdateSkill();
        }
    }

    //---------------------------------------------------

    void StartSkill()
    {
        if (currentCooldown > 0)
            return;

        if (!playerMana.UseMana(manaCost))
            return;

        currentCooldown = cooldown;

        skillTimer = skillDuration;

        spawnTimer = 0;

        isUsingSkill = true;
    }

    //---------------------------------------------------

    void UpdateSkill()
    {
        skillTimer -= Time.deltaTime;

        if (skillTimer <= 0)
        {
            isUsingSkill = false;
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnRate)
            return;

        spawnTimer = 0;

        Ray ray =
            playerCamera.ViewportPointToRay(
                new Vector3(.5f, .5f));

        Vector3 direction = ray.direction;

        GameObject smoke =
            Instantiate(
                smokePrefab,
                smokePoint.position,
                Quaternion.LookRotation(direction));

        SmokeProjectile projectile =
            smoke.GetComponent<SmokeProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                smokeSpeed,
                smokeLifeTime);
        }
        else
        {
            Destroy(smoke, smokeLifeTime);
        }
    }

    //---------------------------------------------------

    void UpdateCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown < 0)
                currentCooldown = 0;

            if (cooldownImage != null)
            {
                cooldownImage.fillAmount =
                    1 -
                    currentCooldown /
                    cooldown;
            }

            if (skillIcon != null)
                skillIcon.color = Color.gray;
        }
        else
        {
            if (cooldownImage != null)
                cooldownImage.fillAmount = 1;

            if (skillIcon != null)
                skillIcon.color = Color.white;
        }
    }
}