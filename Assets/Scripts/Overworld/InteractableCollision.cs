using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            // could later add different events

            // int r = Random.Range(1, 4);
            // if (r == 1)
            //     LevelManager.Instance.LootBox();
            // else if (r == 2)
            //     LevelManager.Instance.MerchantShop();
            // else
            //     LevelManager.Instance.TrapPlayer();

            LootCardGA lootCardGA = new LootCardGA(3); 
            ActionSystem.Instance.Perform(lootCardGA);
            Destroy(this.gameObject);
        }
    }
}
