using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Animator animator;

    void Update()
    {
        bool aiming = Mouse.current != null &&
                      Mouse.current.leftButton.isPressed;

        animator.SetBool("IsAiming", aiming);
    }
}