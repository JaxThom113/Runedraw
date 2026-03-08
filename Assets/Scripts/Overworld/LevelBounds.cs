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
            // Debug.Log("Player entered top bound");
            LevelSystem.Instance.NextLevel();
        }
    }
}