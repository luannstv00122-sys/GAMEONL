using UnityEngine;
using UnityEngine.Rendering;

public class GamePerformance : MonoBehaviour
{
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        OnDemandRendering.renderFrameInterval = 1;
    }
}