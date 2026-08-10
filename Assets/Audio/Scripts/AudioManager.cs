using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    [Header("BGM")]
    [SerializeField] private AudioClip backgroundMusic;


    [Header("UI Audio")]
    [SerializeField] private AudioClip buttonClick;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        StartBackgroundMusic();
    }

    private void StartBackgroundMusic()
    {
        if (bgmSource == null || backgroundMusic == null)
            return;

        bgmSource.clip = backgroundMusic;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
    }

    public void PlayButtonClick()
    {
        if (uiSource != null && buttonClick != null)
        {
            uiSource.PlayOneShot(buttonClick);
        }
    }
}