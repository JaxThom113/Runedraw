using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;
using DG.Tweening;

public class MatchSetupSystem : MonoBehaviour
{
    const string FogHideDistanceProperty = "HideDistance";

    [SerializeField] public PlayerView playerView;
    [SerializeField] public EnemyView enemyView;
    [SerializeField] public GameObject enemyCanvas;
    [SerializeField] public CinemachineVirtualCamera playerCamera;

    [SerializeField] public float hideDistanceLowerBound = 0.3f;
    [SerializeField] public float hideDistanceUpperBound = 5.0f;
    [SerializeField] float hideDistanceTweenDuration = 1f;
    [SerializeField] VisualEffect fogVisualEffect;

    Tween fogHideDistanceTween;

    public void SetupMatch(OverworldEnemy overworldEnemy)
    {
        TweenFogHideDistanceToUpper();
      
        foreach (OverworldEnemy oe in FindObjectsByType<OverworldEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            oe.SetBleedVisualEnabled(false);

        overworldEnemy.SetBleedVisualEnabled(true);

        //playerCamera.Follow = overworldEnemy.SpriteGameObject.transform;
        EnemySO enemyData = overworldEnemy.enemyData; 
        PlayerSystem.Instance.Setup(playerView);
        EnemySystem.Instance.Setup(overworldEnemy); 
        overworldEnemy.ApplyMaterial(EnemySystem.Instance.CurrentEnemyMaterial);
        DamageSystem.Instance.Setup(playerView, enemyView);
        ShieldSystem.Instance.Setup(playerView, enemyView); 
        if (enemyData.entityDialogue != null)
            DialogueSystem.Instance.Setup(enemyData.entityDialogue);
        playerView.Setup(PlayerSystem.Instance.currentPlayerData); 
        
        enemyView.Setup(enemyData, overworldEnemy);  
        
        
         StartCoroutine(SetupCards());
    }
 

    private IEnumerator SetupCards(){  
        yield return new WaitForSeconds(1f); 
        //DialogueSystem.Instance.IntroDialogue();
        List<CardSO> playerDeck = PlayerSystem.Instance.player.playerDeck;  
        List<CardSOList> enemyDeck = EnemySystem.Instance.enemy.enemyDeck; 
        CardSystem.Instance.Setup(playerDeck, enemyDeck); 

        StartRoundGA startRoundGA = new(5, EnemySystem.Instance.GetDrawAmount());
        ActionSystem.Instance.Perform(startRoundGA, () => ManaSystem.Instance.InitializeMana());
    }

    void KillFogHideDistanceTween()
    {
        fogHideDistanceTween?.Kill();
        fogHideDistanceTween = null;
    }

    void TweenFogHideDistanceToUpper()
    {
        if (fogVisualEffect == null)
            return;

        KillFogHideDistanceTween();
        fogVisualEffect.SetFloat(FogHideDistanceProperty, hideDistanceLowerBound);
        fogHideDistanceTween = DOTween.To(
            () => fogVisualEffect.GetFloat(FogHideDistanceProperty),
            x => fogVisualEffect.SetFloat(FogHideDistanceProperty, x),
            hideDistanceUpperBound,
            hideDistanceTweenDuration
        ).SetTarget(this);
    }

    public void BeginFogHideDistanceTweenToLower()
    {
        if (fogVisualEffect == null)
            return;

        KillFogHideDistanceTween();
        fogHideDistanceTween = DOTween.To(
            () => fogVisualEffect.GetFloat(FogHideDistanceProperty),
            x => fogVisualEffect.SetFloat(FogHideDistanceProperty, x),
            hideDistanceLowerBound,
            hideDistanceTweenDuration
        ).SetTarget(this);
    }

    public IEnumerator FogHideDistanceTweenToLowerRoutine()
    {
        BeginFogHideDistanceTweenToLower();
        if (fogHideDistanceTween == null)
            yield break;
        yield return fogHideDistanceTween.WaitForCompletion();
        fogHideDistanceTween = null;
    }

    void OnDestroy()
    {
        KillFogHideDistanceTween();
    }
}
