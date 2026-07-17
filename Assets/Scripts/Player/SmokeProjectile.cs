using UnityEngine;

public class SmokeProjectile : MonoBehaviour
{
    Vector3 direction;

    float speed;

    public void Initialize(
        Vector3 dir,
        float moveSpeed,
        float life)
    {
        direction = dir.normalized;

        speed = moveSpeed;

        Destroy(gameObject, life);
    }

    void Update()
    {
        transform.position +=
            direction *
            speed *
            Time.deltaTime;
    }
}