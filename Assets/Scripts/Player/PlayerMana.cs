using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana = 100f;

    [Header("Mana Regeneration")]
    public float manaRegenPerSecond = 1f;   // Hồi 10 mana mỗi giây

    [Header("UI")]
    public Image manaFill;

    void Start()
    {
        currentMana = maxMana;
        UpdateManaUI();
    }

    void Update()
    {
        RegenerateMana();
    }

    void RegenerateMana()
    {
        if (currentMana >= maxMana)
            return;

        currentMana += manaRegenPerSecond * Time.deltaTime;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        UpdateManaUI();
    }

    public bool UseMana(float amount)
    {
        if (currentMana < amount)
            return false;

        currentMana -= amount;
        UpdateManaUI();

        return true;
    }

    public void AddMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        UpdateManaUI();
    }

    void UpdateManaUI()
    {
        if (manaFill != null)
            manaFill.fillAmount = currentMana / maxMana;
    }
}