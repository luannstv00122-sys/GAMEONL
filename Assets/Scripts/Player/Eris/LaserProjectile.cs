using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float moveSpeed;

    public void Initialize(
        Vector3 direction,
        float speed,
        float lifeTime
    )
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position +=
            moveDirection * moveSpeed * Time.deltaTime;
    }
}