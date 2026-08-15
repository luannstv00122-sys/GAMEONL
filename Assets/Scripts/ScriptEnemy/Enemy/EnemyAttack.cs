using UnityEngine;

public class EnemyAttack : EnemyAttackBase
{
    [Header("Melee")]
    [SerializeField] private Vector3 attackBoxSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private float attackForwardOffset = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Damage")]
    [SerializeField] private float damage = 20f;

    public override void Attack()
    {
        if (!CanAttack())
            return;

        cooldownTimer = attackCooldown;

        animator.SetTrigger("Attack");

        Collider[] targets = MeleeAttack(
            attackBoxSize,
            attackForwardOffset,
            targetLayer
        );

        foreach (Collider target in targets)
        {
            Debug.Log("Hit: " + target.name);

            PlayerHealth playerHealth =
                target.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);

                Debug.Log("Player mất " + damage + " máu");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 center =
            transform.position +
            transform.forward * attackForwardOffset;

        Gizmos.matrix = Matrix4x4.TRS(
            center,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            attackBoxSize
        );

        Gizmos.matrix = Matrix4x4.identity;
    }
}