using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    // check if player enters top bound, if so, load next level
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ActionSystem.Instance == null)
                return;

            if (!ActionSystem.Instance.IsPerforming)
            {
                int nextLevel = LevelSystem.Instance != null ? LevelSystem.Instance.CurrentArea + 1 : 1;
                ActionSystem.Instance.Perform(new NextAreaGA(nextLevel));
            }
        }
    }
}