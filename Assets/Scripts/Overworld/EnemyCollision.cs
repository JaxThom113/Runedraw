using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            OverworldEnemy overworldEnemy = GetComponent<OverworldEnemy>();
            GetComponent<SphereCollider>().enabled = false;
            overworldEnemy.ApplyCurrentEnemyMaterial();
            Debug.Log("Player collided with enemy");
            CameraTransitionSystem.Instance.startGame(overworldEnemy);
        }
    }
}
