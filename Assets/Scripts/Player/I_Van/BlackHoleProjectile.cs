using UnityEngine;

public class BlackHoleProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 20f;

    [Header("Activation")]
    public float flyTimeBeforeActivate = 1f;

    private Vector3 moveDirection;
    private float flyTimer;
    private bool activated;

    private BlackHolePull blackHolePull;

    void Awake()
    {
        blackHolePull = GetComponent<BlackHolePull>();

        if (blackHolePull != null)
            blackHolePull.enabled = false;
    }

    public void Initialize(
        Vector3 direction,
        float projectileSpeed,
        float totalLifeTime
    )
    {
        moveDirection = direction.normalized;
        speed = projectileSpeed;

        Destroy(gameObject, totalLifeTime);
    }

    void Update()
    {
        if (activated)
            return;

        transform.position +=
            moveDirection * speed * Time.deltaTime;

        flyTimer += Time.deltaTime;

        if (flyTimer >= flyTimeBeforeActivate)
        {
            ActivateBlackHole();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        if (!other.CompareTag("Player"))
        {
            ActivateBlackHole();
        }
    }

    void ActivateBlackHole()
    {
        activated = true;

        if (blackHolePull != null)
            blackHolePull.enabled = true;
    }
}