using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float damage = 20f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player trúng đạn!");
            // Gọi PlayerHealth ở đây
        }

        Destroy(gameObject);
    }
}