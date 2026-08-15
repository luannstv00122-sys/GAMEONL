using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "Intro";

    [Header("Button Sounds")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip exitSound;

    private void PlaySound(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }

    // =========================
    // START GAME
    // =========================

    public void StartGame()
    {
        StartCoroutine(StartGameWithSound());
    }

    private IEnumerator StartGameWithSound()
    {
        PlaySound(startSound);

        // Chờ âm thanh phát một chút rồi mới chuyển Scene
        if (startSound != null)
            yield return new WaitForSecondsRealtime(startSound.length);

        SceneManager.LoadScene(gameSceneName);
    }

    // =========================
    // EXIT GAME
    // =========================

    public void QuitGame()
    {
        StartCoroutine(QuitGameWithSound());
    }

    private IEnumerator QuitGameWithSound()
    {
        PlaySound(exitSound);

        // Chờ âm thanh Exit phát xong
        if (exitSound != null)
            yield return new WaitForSecondsRealtime(exitSound.length);

        Debug.Log("Quit Game");

#if UNITY_EDITOR
        // Khi test trong Unity Editor -> dừng Play Mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Khi Build game -> thoát game thật
        Application.Quit();
#endif
    }
}