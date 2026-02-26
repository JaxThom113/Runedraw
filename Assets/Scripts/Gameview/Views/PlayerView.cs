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
    }


}
