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
    public void Setup(DialogueSO dialogueSO)
    {
        this.dialogueSO = dialogueSO; 
        dialogueText.transform.parent.gameObject.SetActive(false);
    }
    void OnEnable()
    {
        ActionSystem.SubscribeReaction<DrawEnemyCardGA>(TurnStartDialogue, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<DiscardCardGA>(AttackDialogue, ReactionTiming.POST); 
        ActionSystem.SubscribeReaction<KillEnemyGA>(LoseDialogue, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<DrawEnemyCardGA>(TurnStartDialogue, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<DiscardCardGA>(AttackDialogue, ReactionTiming.POST); 
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(LoseDialogue, ReactionTiming.POST);
    }
    string GetRandomDialogue(List<string> dialogue)
    {  
        StopAllCoroutines();
        StartCoroutine(DialogueCoroutine());
        return dialogue[Random.Range(0, dialogue.Count)];
    }  
    private IEnumerator DialogueCoroutine(){ 
        dialogueText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        dialogueText.transform.parent.gameObject.SetActive(false);
    }
    public void IntroDialogue()
    {
        dialogueText.text = GetRandomDialogue(dialogueSO.Intro);
    }
    private void TurnStartDialogue(DrawEnemyCardGA drawEnemyCardGA)
    {  
        if(enemyView.currentHealth <= 0) return;
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
        if(enemyView.currentHealth <= 0) return;
        dialogueText.text = GetRandomDialogue(dialogueSO.Attack); 
        StartCoroutine(EndTurnDialogue());
    }
    private IEnumerator EndTurnDialogue()
    {   
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
        dialogueText.text = GetRandomDialogue(dialogueSO.Lose);
    }
    public void TakeDamageDialogue(){ 
        if(enemyView.currentHealth <= 0) return;
        dialogueText.text = GetRandomDialogue(dialogueSO.TakeDamage);
    }
    
    

    
}
