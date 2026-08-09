using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI")]
    [SerializeField] private Image healthFill;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.hKey.wasPressedThisFrame)
            TakeDamage(10f);

        if (Keyboard.current.jKey.wasPressedThisFrame)
            Heal(10f);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();
    }

    public void SetHealthFill(Image image)
    {
        healthFill = image;
        UpdateHealthUI();
    }

    public float GetHealthPercent()
    {
        return maxHealth > 0f ? currentHealth / maxHealth : 0f;
    }

    private void UpdateHealthUI()
    {
        if (healthFill == null)
            return;

        healthFill.fillAmount = GetHealthPercent();
    }
}