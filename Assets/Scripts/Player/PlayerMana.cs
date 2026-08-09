using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana = 100f;

    [Header("Mana Regeneration")]
    public float manaRegenPerSecond = 1f;

    [Header("UI")]
    [SerializeField] private Image manaFill;

    private void Start()
    {
        currentMana = maxMana;
        UpdateManaUI();
    }

    private void Update()
    {
        RegenerateMana();
    }

    private void RegenerateMana()
    {
        if (currentMana >= maxMana)
            return;

        currentMana += manaRegenPerSecond * Time.deltaTime;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        UpdateManaUI();
    }

    public bool UseMana(float amount)
    {
        if (amount <= 0f)
            return true;

        if (currentMana < amount)
            return false;

        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        UpdateManaUI();

        return true;
    }

    public void AddMana(float amount)
    {
        if (amount <= 0f)
            return;

        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        UpdateManaUI();
    }

    public void SetManaFill(Image image)
    {
        manaFill = image;
        UpdateManaUI();
    }

    public float GetManaPercent()
    {
        return maxMana > 0f ? currentMana / maxMana : 0f;
    }

    private void UpdateManaUI()
    {
        if (manaFill == null)
            return;

        manaFill.fillAmount = GetManaPercent();
    }
}