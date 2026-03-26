using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System.Linq;
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] public List<string> Intro; 
    [SerializeField] public List<string> TurnStart;  
    [SerializeField] public List<string> LowHealthTurnStart; 
    [SerializeField] public List<string> Attack; 
    [SerializeField] public List<string> UltimateAttack;
    [SerializeField] public List<string> TakeDamage; 
    [SerializeField] public List<string> EndTurn; 
    [SerializeField] public List<string> LowHealthEndTurn; 
    [SerializeField] public List<string> Lose;
    [SerializeField] public List<string> Win; 
    
}
