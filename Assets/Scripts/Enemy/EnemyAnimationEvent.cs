using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    private EnemyGunAttack gunAttack;

    private void Awake()
    {
        gunAttack = GetComponentInParent<EnemyGunAttack>();
    }

    public void FireBullet()
    {
        gunAttack.FireBullet();
    }
}