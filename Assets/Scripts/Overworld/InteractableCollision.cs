using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with Event");

            // could later add different events

            // int r = Random.Range(1, 4);
            // if (r == 1)
            //     LevelManager.Instance.LootBox();
            // else if (r == 2)
            //     LevelManager.Instance.MerchantShop();
            // else
            //     LevelManager.Instance.TrapPlayer();

            LevelSystem.Instance.LootBox(this.gameObject);
        }
    }
}
