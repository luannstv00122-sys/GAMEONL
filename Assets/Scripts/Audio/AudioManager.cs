using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip buttonHover;
    [SerializeField] private AudioClip inventoryOpen;
    [SerializeField] private AudioClip inventoryClose;
    [SerializeField] private AudioClip buttonError;

    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip pickup;
    [SerializeField] private AudioClip build;
    [SerializeField] private AudioClip upgrade;
    [SerializeField] private AudioClip attack;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // UI
    // =========================

    public void PlayButtonClick()
    {
        PlayUI(buttonClick);
    }

    public void PlayButtonHover()
    {
        PlayUI(buttonHover);
    }

    public void PlayInventoryOpen()
    {
        PlayUI(inventoryOpen);
    }

    public void PlayInventoryClose()
    {
        PlayUI(inventoryClose);
    }

    public void PlayButtonError()
    {
        PlayUI(buttonError);
    }

    // =========================
    // GAMEPLAY
    // =========================

    public void PlayPickup()
    {
        PlaySFX(pickup);
    }

    public void PlayBuild()
    {
        PlaySFX(build);
    }

    public void PlayUpgrade()
    {
        PlaySFX(upgrade);
    }

    public void PlayAttack()
    {
        PlaySFX(attack);
    }

    // =========================
    // INTERNAL
    // =========================

    private void PlayUI(AudioClip clip)
    {
        if (clip != null && uiSource != null)
        {
            uiSource.PlayOneShot(clip);
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}