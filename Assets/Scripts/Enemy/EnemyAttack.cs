using UnityEngine;

public class EnemyAttack : EnemyAttackBase
{
    public override void Attack()
    {
        if (!CanAttack())
            return;

        cooldownTimer = attackCooldown;

        animator.SetTrigger("Attack");

        Collider[] targets = GetTargets();

        foreach (Collider target in targets)
        {
            Debug.Log("Hit: " + target.name);

            // target.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }
}