using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class OverworldHealth : MonoBehaviour
{
   [SerializeField] private Slider healthSlider;  
   [SerializeField] private PlayerSO defaultPlayerData;
   [SerializeField] private TextMeshProUGUI healthText; 
   void OnEnable()
   { 
    if(PlayerSystem.Instance == null)
    {
       healthSlider.value = defaultPlayerData.entityHealth; 
       healthText.text = defaultPlayerData.entityHealth.ToString();
    }
    else
    {
        healthSlider.value = PlayerSystem.Instance.storedHealth;
        healthText.text = PlayerSystem.Instance.storedHealth.ToString();
    }
  
   }
}
