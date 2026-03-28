using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public void OnClick()
    {
        if (ActionSystem.Instance == null || ActionSystem.Instance.IsPerforming) return;
        if (LevelSystem.Instance != null && LevelSystem.Instance.LootView != null && LevelSystem.Instance.LootView.activeInHierarchy) return;
        if (DamageSystem.Instance != null && DamageSystem.Instance.enemyView != null && DamageSystem.Instance.enemyView.currentHealth <= 0) return;

        SoundEffectSystem.Instance.PlayButtonClickSound();
        EnemyTurnGA enemyTurnGA = new(); 
        ActionSystem.Instance.Perform(enemyTurnGA, () =>
        {
            PoisonSystem.Instance?.RefreshBothSides();
            BleedSystem.Instance?.RefreshBothSides();
            VunerableSystem.Instance?.RefreshBothSides();
        }); //always need a performer for game action
    }
}
