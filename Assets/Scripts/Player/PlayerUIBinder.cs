using Fusion;
using UnityEngine;

public class PlayerUIBinder : NetworkBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMana playerMana;

    [Header("Character Skills")]
    [Tooltip("Kéo component skill sử dụng phím Q vào đây.")]
    [SerializeField] private MonoBehaviour skillQComponent;

    [Tooltip("Kéo component skill sử dụng phím E vào đây.")]
    [SerializeField] private MonoBehaviour skillEComponent;

    private IPlayerSkillUI skillQ;
    private IPlayerSkillUI skillE;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerMana == null)
            playerMana = GetComponent<PlayerMana>();

        skillQ = skillQComponent as IPlayerSkillUI;
        skillE = skillEComponent as IPlayerSkillUI;

        if (skillQComponent != null && skillQ == null)
        {
            Debug.LogError(
                $"{name}: Skill Q chưa implement IPlayerSkillUI."
            );
        }

        if (skillEComponent != null && skillE == null)
        {
            Debug.LogError(
                $"{name}: Skill E chưa implement IPlayerSkillUI."
            );
        }

        AssignManaToSkills();
    }

    public override void Spawned()
    {
        // Chỉ character local được sử dụng HUD của máy này.
        if (!Object.HasInputAuthority)
            return;

        BindLocalHUD();
    }

    private void AssignManaToSkills()
    {
        if (playerMana == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy PlayerMana."
            );

            return;
        }

        skillQ?.SetPlayerMana(playerMana);
        skillE?.SetPlayerMana(playerMana);
    }

    private void BindLocalHUD()
    {
        GameplayHUD hud = GameplayHUD.Instance;

        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUD>();

        if (hud == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy GameplayHUD."
            );

            return;
        }

        if (playerHealth != null)
        {
            playerHealth.SetHealthFill(
                hud.HealthFill
            );
        }

        if (playerMana != null)
        {
            playerMana.SetManaFill(
                hud.ManaFill
            );
        }

        if (skillQ != null)
        {
            skillQ.SetSkillUI(
                hud.SkillQCooldownImage,
                hud.SkillQIcon
            );
        }

        if (skillE != null)
        {
            skillE.SetSkillUI(
                hud.SkillECooldownImage,
                hud.SkillEIcon
            );
        }

        Debug.Log(
            $"{name}: Đã gắn Health, Mana, Skill Q và Skill E UI."
        );
    }
}