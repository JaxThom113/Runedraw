using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem>
{ 
    // based off structure from youtube video https://www.youtube.com/watch?v=ls5zeiDCfvI
   private List<GameAction> reactions = null; //the state of the current flow/contains the reactions for the current state (pre, perform, or post)

   public bool IsPerforming { get; private set;} = false; 

   private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();  
   //When you draw a card, you can have a reaction to the card before being drawn

    //Action<T> is a delegate that returns void 
    //List that stores methods that take a GameAction as an argument and return void
   private static Dictionary<Type, List<Action<GameAction>>> postSubs = new(); 
   //When you draw a card, you can have a reaction to the card after being drawn

    //NO LIST, one function per type
   private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new(); // inbetween pre-post subs GA application, generic form
   //Called when an action system is performing an action


/* 
ACTION SYSTEM ARCHITECTURE OVERVIEW

Entry: Someone calls ActionSystem.Instance.Perform(gameAction)
Flow: Perform → Flow(action) → PRE phase → PERFORM phase → POST phase

PRE phase: action.PreReactions, preSubs callbacks, then PerformReactions (recursive Flow for each)
PERFORM phase: action.PerformReactions, PerformPerformer(action), then PerformReactions (recursive)
POST phase: action.PostReactions, postSubs callbacks, then PerformReactions (recursive)

Three concepts:
1. PERFORMERS - One per GameAction type. Define HOW that action executes. Registered via AttachPerformer<T>. Systems only.
2. SUBSCRIBERS - Callbacks per type. Invoked BEFORE (pre) or AFTER (post) an action runs. Registered via SubscribeReaction<T>. Observer pattern. Can AddReaction.
3. REACTIONS - Child GameActions on PreReactions, PerformReactions, PostReactions. Run via recursive Flow. Added via AddReaction (from performer/subscriber) or pre-populated on GameAction.

Key rule: Cannot call Perform from inside a performer (IsPerforming blocks it). Use AddReaction instead. 
*/

//----------------------------------------------------
// ACTION SYSTEM (main orchestrator)
// 1. Receives an action.
// 2. Executes PreReactions.
// 3. Invokes performer (core logic).
// 4. Executes PerformReactions.
// 5. Executes PostReactions.
//----------------------------------------------------

/*
Perform(action)  →  Flow(action)
                        │
                        ├─ PRE:    action.PreReactions (nested Flows)  →  preSubs callbacks
                        ├─ PERFORM: Performer(action)  →  action.PerformReactions (nested Flows)
                        └─ POST:   postSubs callbacks  →  action.PostReactions (nested Flows) 

Flow is the MAIN METHOD of the action system that orchestrates the execution of the action system. 
    a recursive method that calls itself to execute the pre reactions,  
    then when done with pre then the performer,  
    and then when done with perform then the post reactions, and then done with post reactions we call the callback set from the perform method.
    It is called by the PERFORM method, and it will execute the pre reactions, the performer, and the post reactions.
    It will also execute the pre subscribers, the performer, and the post subscribers.
*/



//----------------------------------------------------
// PERFORMERS (systems only)
// One per GameAction type. Define HOW that action type is executed (core logic). 
// Calls the flow, which executes the pre reactions, the performer, and the post reactions from the subscribers
// Registered via AttachPerformer<T>; invoked in the middle of Flow (between pre and post).
//----------------------------------------------------

//----------------------------------------------------
// SUBSCRIBERS (preSubs / postSubs)
// Hold callbacks per action type. WHEN an action runs, we invoke them BEFORE (pre) or AFTER (post). 
// Buckets the reactions for the action type, and executes them when the action is performed, based on the timing
// Multiple callbacks per type; used for side effects (UI, logging, etc.) without owning execution.
//----------------------------------------------------

//----------------------------------------------------
// REACTIONS (on GameAction: PreReactions, PerformReactions, PostReactions)
// Child GameActions that run at each phase. Define WHAT runs and WHEN (pre / perform / post).
// Each reaction is executed via Flow; used to chain or compose actions (e.g. status effects).
//---------------------------------------------------- 
   /// <returns>False if another action is already running (call was ignored).</returns>
   public bool Perform(GameAction action, System.Action OnPerformFinished = null) 
   { 
        if(IsPerforming) return false;  // makes sure we dont perform the same action twice
        IsPerforming = true;  
        StartCoroutine(Flow(action, () =>  
        {  
            //when flow is done call callback
            IsPerforming = false;  
            OnPerformFinished?.Invoke(); //if we want to have our own callback, we can add it here, perform
        }));
        return true;
   } 
   public void AddReaction(GameAction gameAction) 
   { 
         reactions?.Add(gameAction); // adds one reaction to the current phase's list (pre, perform, or post)
   }  
   //Where the magic happens, when reactions added to subscribers, this is where the reactions are PERFORMED
   private IEnumerator Flow(GameAction action, System.Action OnFlowFinished = null) 
   {  
       //PRE REACTIONS
       reactions = action.PreReactions; //set reactions to pre reactions list of our current action 
       //now all actions can react to the current action 
       PerformSubscribers(action, preSubs); //if added to preSub, perform preSub's at the BEGINNING 
       //Akin to performPerformer, but for the pre subscribers 
       //The execution of flow
       yield return PerformReactions(); //recursive method call to itself inorder to call all reactions


       //PERFORM REACTIONS
       reactions = action.PerformReactions; 
       yield return PerformPerformer(action); //perform the performer for the current action 
       //the execution of flow
       yield return PerformReactions(); //recursive method call to itself inorder to call all reactions


       //POST REACTIONS
       reactions = action.PostReactions; 
       PerformSubscribers(action, postSubs); //if added to postSub, perform postSub's at the END 
       //Akin to performPerformer, but for the post subscribers 
       yield return PerformReactions(); //recursive method call to itself inorder to call all reactions

       OnFlowFinished?.Invoke(); //simply invoking callback
   } 
   private IEnumerator PerformPerformer(GameAction action)  
   {   
    //IE: DrawCardGA is called here
    //EXECUTION BABY!!!
          Type type = action.GetType(); //IE: getting the type DrawCardGA
          if (performers.ContainsKey(type)) //check if type was added to dictionary in the SYSTEM script (IE: CardSystem)
          { 
            yield return performers[type](action);
          }
   } 
   private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs) 
   {  
    //tells all pre subscribers of this action type that we are performing this action type
        Type type = action.GetType(); 
        if (subs.ContainsKey(type)) 
        { 
            foreach(var sub in subs[type]) 
            { 
                sub(action);
            }
        }
   } 
   private IEnumerator PerformReactions() //perform all reactions for the current action via recursion
   {   
    foreach(var reaction in reactions)
        {
            yield return Flow(reaction);
        }
   }  
  //If you only want to perform the type once
   public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction 
   {    
        //ADDS PERFORMER TO DICTIONARY FROM THE SYSTEM SCRIPT(S) (IE: CardSystem)
        //IEnumerator as sometimes we want to wait before action is performed
        //this method does the logic of our game action, and attatches it to the action system 
        Type type = typeof(T);  //first get the type of the game action (different classes have different types)
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action); //Convert our performer to an wrappedPerformer so it can be added to the dictionary
        if (performers.ContainsKey(type)) performers[type] = wrappedPerformer; //then simply add it to the dictionary
        else performers.Add(type, wrappedPerformer); 
        
   } 
   public static void DetachPerformer<T>() where T : GameAction 
   {  
        //Detaches a performer
        Type type = typeof(T); 
        if (performers.ContainsKey(type)) performers.Remove(type); 
   }  
   //Reaction are for the status effects
   public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction 
   {  
        //simple function, adds a reaction to the dictionary based on the timing (before or after the action is performed)
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;  //subscribes based on the timing inputted
        void wrappedReaction(GameAction action) => reaction((T)action);  
        //Defining the function, so reaction((T)action) IE: EnemyTurnPreReaction(EnemyTurnGA) is not being called
        if (subs.ContainsKey(typeof(T))) 
        { 
            subs[typeof(T)].Add(wrappedReaction);
        } 
        else 
        { 
            subs.Add(typeof(T), new()); 
            subs[typeof(T)].Add(wrappedReaction);
        }
   } 
   public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction  
   {  
        //simple function, removes a reaction from the dictionary based on the timing (before or after the action is performed)
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;  //creats a subscription dictionary based on the timing (before or after the action is performed)
        if (subs.ContainsKey(typeof(T))) 
        { 
            void wrappedReaction(GameAction action) => reaction((T)action);  //creates a function that wraps the reaction function so it can be removed from the dictionary
            subs[typeof(T)].Remove(wrappedReaction);
        }
   } 
}
