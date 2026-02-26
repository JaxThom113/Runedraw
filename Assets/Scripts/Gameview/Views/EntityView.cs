using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EntityView : MonoBehaviour
{
    [SerializeField] public TMP_Text healthText;
    [SerializeField] public Slider healthSlider; 
    [SerializeField] public Animator animator;

    public int maxHealth;
    public int currentHealth; 
    public bool firstSetup = true;

    protected void SetupBase(EntitySO entityData)
    { 
        if(firstSetup)
        { 
            maxHealth = currentHealth = entityData.entityHealth;
            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
            }
            
            firstSetup = false; 
            UpdateHealthDisplay(); 
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
    public void ReduceHealth(int amount)
    {  
        if(animator != null)
        {
            animator.SetTrigger("Hurt");
        }
        currentHealth -= amount;
        UpdateHealthDisplay();
    }
}
