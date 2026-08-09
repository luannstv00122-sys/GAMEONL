using Fusion;
using UnityEngine;

public class NetworkPlayerSetup : NetworkBehaviour
{
    [Header("Local Player Settings")]
    [SerializeField] private MonoBehaviour[] localOnlyScripts;

    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    public override void Spawned()
    {
        bool isLocal = Object.HasInputAuthority;

        foreach (MonoBehaviour script in localOnlyScripts)
        {
            if (script != null)
                script.enabled = isLocal;
        }

        if (!isLocal)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            if (audioListener != null)
                audioListener.enabled = false;

            return;
        }

        SetupLocalCamera();

        Debug.Log(
            $"NetworkPlayerSetup Spawned - Local={isLocal}, " +
            $"Player={Object.InputAuthority.PlayerId}"
        );
    }

    private void SetupLocalCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;

            if (playerCamera == null)
                playerCamera = FindFirstObjectByType<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogError("Không tìm thấy Camera trong scene.");
            return;
        }

        playerCamera.gameObject.SetActive(true);

        if (audioListener == null)
            audioListener = playerCamera.GetComponent<AudioListener>();

        if (audioListener != null)
            audioListener.enabled = true;

        vThirdPersonCamera thirdPersonCamera =
            playerCamera.GetComponent<vThirdPersonCamera>();

        if (thirdPersonCamera == null)
        {
            Debug.LogError(
                "Camera không có component vThirdPersonCamera."
            );
            return;
        }

        thirdPersonCamera.SetMainTarget(transform);

        Debug.Log(
            $"Đã gán camera Target = {gameObject.name}"
        );
    }
}