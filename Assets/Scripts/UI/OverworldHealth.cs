using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class OverworldHealth : MonoBehaviour
{
   [SerializeField] private PlayerSO defaultPlayerData;
   [SerializeField] private Slider healthSlider;  
   [SerializeField] private TextMeshProUGUI healthText; 

   void OnEnable()
   { 
      healthSlider.maxValue = defaultPlayerData.entityHealth;

      if (PlayerSystem.Instance == null)
      {
         healthSlider.value = defaultPlayerData.entityHealth; 
         healthText.text = $"{defaultPlayerData.entityHealth} / {healthSlider.maxValue}";
      }
      else
      {
         healthSlider.value = PlayerSystem.Instance.storedHealth;
         healthText.text = $"{PlayerSystem.Instance.storedHealth} / {healthSlider.maxValue}";
      }
   }
}
