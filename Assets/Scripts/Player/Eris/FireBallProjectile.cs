using UnityEngine;

public class FireBallProjectile : MonoBehaviour
{
    [Header("Movement")]
    private Vector3 moveDirection;
    private float moveSpeed;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float explosionLifeTime = 5f;
    public Vector3 explosionOffset = Vector3.zero;

    [Header("Collision")]
    public LayerMask hitLayers;

    private bool hasExploded;

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
        if (hasExploded)
            return;

        transform.position +=
            moveDirection * moveSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
            return;

        bool isHitLayer =
            (hitLayers.value & (1 << other.gameObject.layer)) != 0;

        if (!isHitLayer)
            return;

        Explode();
    }

    void Explode()
    {
        hasExploded = true;

        if (explosionPrefab != null)
        {
            Vector3 spawnPosition =
                transform.position + explosionOffset;

            GameObject explosion = Instantiate(
                explosionPrefab,
                spawnPosition,
                Quaternion.identity
            );

            Destroy(explosion, explosionLifeTime);
        }

        Destroy(gameObject);
    }
}