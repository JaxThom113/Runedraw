using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EntityView : MonoBehaviour
{
    [SerializeField] public TMP_Text healthText;
    [SerializeField] public Slider healthSlider;  
    [SerializeField] public TMP_Text shieldText; 
    [SerializeField] public Slider shieldSlider;
    [SerializeField] public Animator animator;

    public int maxShield;
    public int currentShield;
    public int maxHealth;
    public int currentHealth; 
    public bool firstSetup = true;

    protected void SetupBase(EntitySO entityData)
    { 
        if(firstSetup)
        { 
            maxHealth = currentHealth = entityData.entityHealth;
            currentShield = 0;
            maxShield = 0;
            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
            }
            firstSetup = false; 
            UpdateHealthDisplay();
            UpdateShieldDisplay();
            return;
        } 
    }

    private void UpdateHealthDisplay()
    {
        if (healthText != null)
            healthText.text = $"{currentHealth}";
        if (healthSlider != null && maxHealth > 0)
            healthSlider.value = (float)currentHealth / maxHealth;
    }

    private void UpdateShieldDisplay()
    {
        if (shieldText != null)
            shieldText.text = $"{currentShield}";
        bool hasShield = currentShield > 0;
        if (shieldSlider != null)
            shieldSlider.gameObject.SetActive(hasShield);
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(!hasShield);
    }


    public void TakeDamage(int amount)
    {
        int toShield = Mathf.Min(amount, currentShield);
        currentShield -= toShield;
        int toHealth = amount - toShield;
        if (toHealth > 0)
        {
            if (animator != null)
                animator.SetTrigger("Hurt");
            currentHealth -= toHealth;
            UpdateHealthDisplay();
        }
        UpdateShieldDisplay();
    }


    public void ClearShield()
    {
        currentShield = 0;
        UpdateShieldDisplay();
    }


    public void AddShield(int amount)
    {
        currentShield += amount;
        if (maxShield > 0)
            maxShield = Mathf.Max(maxShield, currentShield);
        else
            maxShield = currentShield;
        UpdateShieldDisplay();
    }


    public void ReduceHealth(int amount)
    {
        if (animator != null)
            animator.SetTrigger("Hurt");
        currentHealth -= amount;
        UpdateHealthDisplay();
    }
}
