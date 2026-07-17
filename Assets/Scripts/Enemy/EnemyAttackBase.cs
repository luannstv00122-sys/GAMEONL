using UnityEngine;

public abstract class EnemyAttackBase : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] protected float attackCooldown = 1.5f;

    [Header("Attack Area")]
    public Vector3 attackBoxSize = new Vector3(2f, 2f, 2f);
    public float attackForwardOffset = 1.5f;
    public LayerMask targetLayer;

    protected float cooldownTimer;
    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public bool CanAttack()
    {
        return cooldownTimer <= 0f;
    }

    protected Collider[] GetTargets()
    {
        Vector3 center = transform.position + transform.forward * attackForwardOffset;

        return Physics.OverlapBox(
            center,
            attackBoxSize * 0.5f,
            transform.rotation,
            targetLayer
        );
    }

    public abstract void Attack();

    private void OnDrawGizmos()
{
    Gizmos.color = Color.red;

    Vector3 center = transform.position + transform.forward * attackForwardOffset;

    Gizmos.matrix = Matrix4x4.TRS(
        center,
        transform.rotation,
        Vector3.one
    );

    Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
}
}