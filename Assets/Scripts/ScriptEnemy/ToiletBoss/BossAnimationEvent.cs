using UnityEngine;

public class BossAnimationEvent : MonoBehaviour
{
    private BossAttack bossAttack;

    private void Awake()
    {
        bossAttack = GetComponentInParent<BossAttack>();
    }

    // =========================
    // COMBO
    // =========================

    // Gọi ở frame cuối Attack4
    public void EndCombo()
    {
        if (bossAttack != null)
            bossAttack.FinishAttack();
    }
}