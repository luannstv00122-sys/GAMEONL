using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Player đã tới cửa cuối!");

        if (GameResultManager.Instance != null)
        {
            GameResultManager.Instance.Win();
        }
    }
}