using UnityEngine;

public class SceneBGMVolume : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(volume);
        }
    }
}