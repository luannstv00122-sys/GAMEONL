using UnityEngine;
using UnityEngine.UI;

public class GameplayHUD : MonoBehaviour
{
    public static GameplayHUD Instance { get; private set; }

    [Header("Health And Mana")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image manaFill;

    [Header("Skill Q")]
    [SerializeField] private Image skillQCooldownImage;
    [SerializeField] private Image skillQIcon;

    [Header("Skill E")]
    [SerializeField] private Image skillECooldownImage;
    [SerializeField] private Image skillEIcon;

    public Image HealthFill => healthFill;
    public Image ManaFill => manaFill;

    public Image SkillQCooldownImage =>
        skillQCooldownImage;

    public Image SkillQIcon =>
        skillQIcon;

    public Image SkillECooldownImage =>
        skillECooldownImage;

    public Image SkillEIcon =>
        skillEIcon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}