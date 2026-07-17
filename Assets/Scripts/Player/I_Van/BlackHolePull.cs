using UnityEngine;

public class BlackHolePull : MonoBehaviour
{
    [Header("Pull Settings")]
    public float pullRadius = 8f;
    public float pullForce = 12f;
    public float activeDuration = 5f;

    [Header("Enemy Detection")]
    public LayerMask enemyLayer;

    private float timer;

    void OnEnable()
    {
        timer = activeDuration;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        PullEnemies();

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void PullEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            pullRadius,
            enemyLayer
        );

        foreach (Collider enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;

            Vector3 direction =
                transform.position - enemyTransform.position;

            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance <= 0.1f)
                continue;

            Rigidbody enemyRb =
                enemy.GetComponentInParent<Rigidbody>();

            if (enemyRb != null && !enemyRb.isKinematic)
            {
                enemyRb.AddForce(
                    direction.normalized * pullForce,
                    ForceMode.Acceleration
                );
            }
            else
            {
                enemyTransform.position = Vector3.MoveTowards(
                    enemyTransform.position,
                    transform.position,
                    pullForce * Time.deltaTime
                );
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}