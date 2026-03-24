using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Entity : MonoBehaviour
{
    public string entityName => data.entityName; 
    public Sprite entityIcon => data.entityIcon; 
    public int entityHealth => data.entityHealth; 
    public DialogueSO entityDialogue => data.entityDialogue;
    

    protected EntitySO data;
   

    protected Entity() { }

    protected void SetupBase(EntitySO dataSO)
    {
        data = dataSO;
    }
}
