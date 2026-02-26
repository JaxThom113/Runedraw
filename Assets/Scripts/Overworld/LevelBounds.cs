using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Player entered top bound");
            LevelSystem.Instance.NextLevel();
        }
    }
}