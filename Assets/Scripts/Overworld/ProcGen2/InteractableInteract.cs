using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableInteract : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with Interactable");

            // could later add different interactables
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
