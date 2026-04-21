using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : EntityView
{
    public void Setup(PlayerSO playerData) { 
        SetupBase(playerData);
    } 
    private void OnEnable()
    { 
        currentHealth = PlayerSystem.Instance.storedHealth;
        // SetupBase only refreshes the UI on the first battle (firstSetup guard),
        // so without this the slider/text keeps stale values on re-entry until the
        // next damage tick. Push the current values to the UI every time we enable.
        if (maxHealth <= 0) maxHealth = PlayerSystem.Instance.maxHealth;
        UpdateHealthDisplay();
    }


}
