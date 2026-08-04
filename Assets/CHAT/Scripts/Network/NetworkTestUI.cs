using Unity.Netcode;
using UnityEngine;

public class NetworkTestUI : MonoBehaviour
{
    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Không tìm thấy NetworkManager.");
            return;
        }

        NetworkManager.Singleton.StartHost();

        Debug.Log("Đã khởi động HOST");
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Không tìm thấy NetworkManager.");
            return;
        }

        NetworkManager.Singleton.StartClient();

        Debug.Log("Đã khởi động CLIENT");
    }

    public void Shutdown()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();

            Debug.Log("Đã ngắt kết nối.");
        }
    }
}