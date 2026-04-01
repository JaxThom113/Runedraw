using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OverworldEnemy : MonoBehaviour
{
    public GameObject SpriteGameObject;  
    public EnemySO enemyData;    

    private Material material; 

    public float fadeInDuration = 5f;
    public void UpdateEnemy(EnemySO enemyData)
    { 
        if(SpriteGameObject.GetComponent<SpriteRenderer>() == null) { 
           Debug.LogError("FIXME new system with shaders instead of sprites");
        } 
        
        material = SpriteGameObject.GetComponent<MeshRenderer>().material; 
        this.enemyData = enemyData;
    } 

    public void FadeIn() 
    {
        material.SetFloat("_FadeIn", 0.44f); 

        DOTween.To( 
            () => material.GetFloat("_FadeIn"), 
            (float x) => material.SetFloat("_FadeIn", x), 
            -6.39f, 
            fadeInDuration
        ).SetEase(Ease.InOutSine);
    }
}
