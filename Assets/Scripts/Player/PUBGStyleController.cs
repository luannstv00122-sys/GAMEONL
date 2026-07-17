using UnityEngine;
using Invector.vCharacterController;

public class PUBGStyleController : MonoBehaviour
{
    private vThirdPersonController controller;

    void Start()
    {
        controller = GetComponent<vThirdPersonController>();
    }

    void Update()
    {
        if (controller == null) return;

        controller.isStrafing = true;
    }
}