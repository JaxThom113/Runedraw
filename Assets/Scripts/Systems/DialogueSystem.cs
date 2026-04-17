using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System.Linq; 
using TMPro;

public class DialogueSystem : Singleton<DialogueSystem>
{ 
    public TMP_Text dialogueText;   
    public EnemyView enemyView;
    private DialogueSO dialogueSO;
    private bool isFirstTurn = true;
    public void Setup(DialogueSO dialogueSO)
    {
        this.dialogueSO = dialogueSO; 
        isFirstTurn = true;
        if (dialogueText != null && dialogueText.transform.parent != null)
            dialogueText.transform.parent.gameObject.SetActive(false);
    }
    void OnEnable()
    {
        ActionSystem.SubscribeReaction<DrawEnemyCardGA>(TurnStartDialogue, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<DiscardCardGA>(AttackDialogue, ReactionTiming.POST); 
        ActionSystem.SubscribeReaction<PlayEnemyCardGA>(UltimateAttackDialogue, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<KillEnemyGA>(LoseDialogue, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<DrawEnemyCardGA>(TurnStartDialogue, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<DiscardCardGA>(AttackDialogue, ReactionTiming.POST); 
        ActionSystem.UnsubscribeReaction<PlayEnemyCardGA>(UltimateAttackDialogue, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(LoseDialogue, ReactionTiming.POST);
    }
    string GetRandomDialogue(List<string> dialogue)
    {  
        if (dialogue == null || dialogue.Count == 0)
            return string.Empty;
        StopAllCoroutines();
        StartCoroutine(DialogueCoroutine());
        return dialogue[Random.Range(0, dialogue.Count)];
    }  
    private IEnumerator DialogueCoroutine(){ 
        if (dialogueText == null || dialogueText.transform.parent == null) yield break;
        dialogueText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        dialogueText.transform.parent.gameObject.SetActive(false);
    }
    public void IntroDialogue()
    {
        if (dialogueSO == null || dialogueText == null) return;
        dialogueText.text = GetRandomDialogue(dialogueSO.Intro);
    }
    private void TurnStartDialogue(DrawEnemyCardGA drawEnemyCardGA)
    {  
        if (dialogueSO == null || dialogueText == null || enemyView == null) return;
        if(enemyView.currentHealth <= 0) return;
        if(isFirstTurn)
        {
            isFirstTurn = false;
            dialogueText.text = GetRandomDialogue(dialogueSO.Intro);
            return;
        }
        if(enemyView.currentHealth <= enemyView.maxHealth * 0.5f)
        {
            dialogueText.text = GetRandomDialogue(dialogueSO.LowHealthTurnStart);
        }
        else
        {
            dialogueText.text = GetRandomDialogue(dialogueSO.TurnStart);
        }
    }    
    private void AttackDialogue(DiscardCardGA discardCardGA)
    {  
        if (dialogueSO == null || dialogueText == null || enemyView == null) return;
        if(enemyView.currentHealth <= 0) return;
        dialogueText.text = GetRandomDialogue(dialogueSO.Attack); 
        StartCoroutine(EndTurnDialogue());
    }
    // Overwrites the generic Attack line from AttackDialogue when the enemy actually plays
    // an ultimate card. DiscardCardGA fires at enemy turn start (before cards play), then each
    // PlayEnemyCardGA fires per card — if any is ultimate, we swap the dialogue in place.
    private void UltimateAttackDialogue(PlayEnemyCardGA playEnemyCardGA)
    {
        if (dialogueSO == null || dialogueText == null || enemyView == null) return;
        if (enemyView.currentHealth <= 0) return;
        if (playEnemyCardGA == null || playEnemyCardGA.card == null || !playEnemyCardGA.card.IsUltimate) return;
        dialogueText.text = GetRandomDialogue(dialogueSO.UltimateAttack);
    }
    private IEnumerator EndTurnDialogue()
    {   
        if (dialogueSO == null || dialogueText == null || enemyView == null) yield break;
        if(enemyView.currentHealth <= 0) yield break;
        yield return new WaitForSeconds(3f); 
        if(enemyView.currentHealth <= enemyView.maxHealth * 0.5f)
        {
            dialogueText.text = GetRandomDialogue(dialogueSO.LowHealthEndTurn);
        }
        else
        {
            dialogueText.text = GetRandomDialogue(dialogueSO.EndTurn);
        }
    } 
    private void LoseDialogue(KillEnemyGA killEnemyGA)
    {
        if (dialogueSO == null || dialogueText == null) return;
        dialogueText.text = GetRandomDialogue(dialogueSO.Lose);
    }
    public void TakeDamageDialogue(){ 
        if (dialogueSO == null || dialogueText == null || enemyView == null) return;
        if(enemyView.currentHealth <= 0) return;
        dialogueText.text = GetRandomDialogue(dialogueSO.TakeDamage);
    }
    
    

    
}
