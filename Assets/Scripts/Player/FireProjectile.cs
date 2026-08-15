using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;

    public float damage = 25f;

    public void SetDirection(Vector3 direction, float projectileSpeed, float lifeTime)
    {
        moveDirection = direction.normalized;
        speed = projectileSpeed;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ gây damage cho object có Tag Enemy
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                Debug.Log("Bắn trúng Enemy: " + other.name);
            }

            Destroy(gameObject);
        }
    }
}