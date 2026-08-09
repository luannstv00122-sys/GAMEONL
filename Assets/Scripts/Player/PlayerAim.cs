using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void SetNetworkAim(bool aiming)
    {
        if (animator != null)
            animator.SetBool("IsAiming", aiming);
    }
}