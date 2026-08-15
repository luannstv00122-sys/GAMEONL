using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [SerializeField] private float damage = 20f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);

            Debug.Log("Kiếm đánh trúng Player - mất " + damage + " HP");
        }
    }
}