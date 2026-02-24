using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInteract3D : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player collided with enemy");
            CameraTransitionSystem.Instance.startGame(GetComponent<OverworldEnemy>());
        }
    }
}
