using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "Lobby";

    private void Awake()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("IntroManager: Chưa gắn VideoPlayer.");
            return;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Start()
    {
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        LoadLobby();
    }

    public void SkipVideo()
    {
        LoadLobby();
    }

    private void LoadLobby()
    {
        SceneManager.LoadScene(lobbySceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}