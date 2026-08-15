using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 3f;

    // Mỗi viên đạn gây 5 damage
    public float damage = 5f;

    [Header("Bullet Sound")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootVolume = 1f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(
                shootSound,
                transform.position,
                shootVolume
            );
        }

        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bullet hit: {other.name}");

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);

            Debug.Log("Player mất " + damage + " máu");
        }

        Destroy(gameObject);
    }
}