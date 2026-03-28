using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTrackSystem : Singleton<CardTrackSystem>
{ 
    public void OnEnable()
    {
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<PlayEnemyCardGA>(PlayEnemyCardPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
    }
    public void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<PlayEnemyCardGA>(PlayEnemyCardPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<StartRoundGA>(StartRoundPreReaction, ReactionTiming.PRE);
    }
    Dictionary<Element, int> playerCardTrackElements = new Dictionary<Element, int>();
    Dictionary<Element, int> enemyCardTrackElements = new Dictionary<Element, int>();
 
    public void PlayCardPostReaction(PlayCardGA playCardGA)
    {
        Element element = playCardGA.card.cardElement;
        playerCardTrackElements[element] = playerCardTrackElements.TryGetValue(element, out int elementCount) ? elementCount + 1 : 1;
    }

    public void PlayEnemyCardPostReaction(PlayEnemyCardGA playEnemyCardGA)
    {
        Element element = playEnemyCardGA.card.cardElement;
        enemyCardTrackElements[element] = enemyCardTrackElements.TryGetValue(element, out int elementCount) ? elementCount + 1 : 1;
    }

    public void StartRoundPreReaction(StartRoundGA startRoundGA)
    {
        playerCardTrackElements.Clear();
        enemyCardTrackElements.Clear();
        ActionSystem.Instance?.AddReaction(new UpdateApplyCardGA());
    } 
    public int GetPlayerCardTrackElement(Element element)
    {
        return playerCardTrackElements.TryGetValue(element, out int count) ? count : 0;
    }
    public int GetEnemyCardTrackElement(Element element)
    {
        return enemyCardTrackElements.TryGetValue(element, out int count) ? count : 0;
    }
}
