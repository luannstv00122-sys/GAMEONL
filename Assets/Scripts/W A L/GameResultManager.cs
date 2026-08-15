using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Settings")]
    [SerializeField] private float returnDelay = 3f;

    private bool gameEnded = false;

    private void Awake()
    {
        Instance = this;

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);
    }

    // =========================
    // WIN
    // =========================
    public void Win()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        if (losePanel != null)
            losePanel.SetActive(false);

        Debug.Log("WIN!");

        StartCoroutine(ReturnAfterDelay());
    }

    // =========================
    // LOSE
    // =========================
    public void Lose()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (losePanel != null)
            losePanel.SetActive(true);

        if (winPanel != null)
            winPanel.SetActive(false);

        Debug.Log("LOSE!");

        StartCoroutine(ReturnAfterDelay());
    }

    // =========================
    // WAIT 3 SECONDS
    // =========================
    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        ReturnToMapSelection();
    }

    // =========================
    // RETURN TO LOBBY
    // =========================
    private void ReturnToMapSelection()
    {
        Debug.Log("Đang quay về Map Selection...");

        // Ghi nhớ rằng Lobby phải mở Map Selection
        ReturnToMapFlag.ReturnToMapSelection = true;

        NetworkRunner runner =
            FindFirstObjectByType<NetworkRunner>();

        // Multiplayer Fusion
        if (runner != null && runner.IsRunning)
        {
            // Server/Host chịu trách nhiệm đổi scene
            if (runner.IsServer)
            {
                runner.LoadScene(
                    SceneRef.FromIndex(2),
                    LoadSceneMode.Single
                );
            }

            return;
        }

        // Chạy test offline
        SceneManager.LoadScene("Lobby");
    }
}