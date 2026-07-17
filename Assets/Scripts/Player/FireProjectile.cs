using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;

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
    public float damage = 25f;
}