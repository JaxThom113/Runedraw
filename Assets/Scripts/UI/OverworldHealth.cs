using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class OverworldHealth : MonoBehaviour
{
   [SerializeField] private Slider healthSlider;  
   [SerializeField] private TextMeshProUGUI healthText; 

   private int previousHealth;

   void Start()
   { 
      healthSlider.maxValue = PlayerSystem.Instance.currentPlayerData.entityHealth;
   }

   void Update()
   {
      // update once whenever health has changed
      if (previousHealth != PlayerSystem.Instance.storedHealth)
      {
         healthSlider.value = PlayerSystem.Instance.storedHealth;
         healthText.text = $"{PlayerSystem.Instance.storedHealth} / {healthSlider.maxValue}";

         previousHealth = PlayerSystem.Instance.storedHealth;
      }
   }
}
