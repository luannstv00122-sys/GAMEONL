// using UnityEngine;
// using UnityEngine.UI;

// public class UIAudioController : MonoBehaviour
// {
//     private void Start()
//     {
//         Button[] buttons = GetComponentsInChildren<Button>(true);

//         foreach (Button button in buttons)
//         {
//             button.onClick.AddListener(PlayButtonClick);
//         }
//     }

//     private void PlayButtonClick()
//     {
//         if (AudioManager.Instance != null)
//         {
//             AudioManager.Instance.PlayButtonClick();
//         }
//     }
// }
using UnityEngine;
using UnityEngine.UI;

public class UIAudioController : MonoBehaviour
{
    private void Awake()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(PlayButtonClick);
        }
    }

    private void OnDestroy()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            button.onClick.RemoveListener(PlayButtonClick);
        }
    }

    private void PlayButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}