using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public void OnClick()
    {
        SoundEffectSystem.Instance.PlayButtonClickSound();
        EnemyTurnGA enemyTurnGA = new(); 
        ActionSystem.Instance.Perform(enemyTurnGA); //always need a performer for game action
    }
}
