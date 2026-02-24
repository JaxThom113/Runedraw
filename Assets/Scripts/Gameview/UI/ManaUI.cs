using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Animations;
public class ManaUI : MonoBehaviour
{   
    
    [SerializeField] public TextMeshProUGUI manaText; 
    public Transform ManaNodes;   
    public Animator ManaIcon;
    float animationLength = 0;
    private List<Animator> animators = new List<Animator>(); 
    private List<Animator> goldAnimators = new List<Animator>();
    private int manaAmount = 0; 

    // Start is called before the first frame update 
    void OnEnable() { 
        
    }
    public void UpdateMana(int manaAmount)
    {
        this.manaAmount = manaAmount;
        manaText.text = manaAmount.ToString();
       
    } 
     
    public IEnumerator SpendManaCoroutine(int manaAmount)
    {  
       
        ManaIcon.Play("ManaIdle");
        for(int i = 0; i< manaAmount; i++)
        {
            animators.Last().Play("FullToGold"); 
            goldAnimators.Add(animators.Last());
            animators.RemoveAt(animators.Count - 1);
        } 
        this.manaAmount = manaAmount; 
        yield return new WaitForSeconds(animationLength);  
        
    }
     
    public IEnumerator StartRound() 
    {  
       
        for(int i = 0; i< ManaNodes.childCount; i++)
        { 
            if(i < manaAmount){  
                var anim = ManaNodes.GetChild(i).GetComponent<Animator>();
                anim.Rebind();
                anim.Play("GreyToGold"); 
                animators.Add(anim);
                animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
            }
        } 
        yield return new WaitForSeconds(animationLength);  
        ManaIcon.Play("ManaIdle");
        for(int i = 0; i< animators.Count; i++)
        { 
            animators[i].Play("GoldToFull");
        } 
        yield return new WaitForSeconds(animationLength); 
        
    } 
    public void ResetMana(int maxMana)
    {
        this.manaAmount = maxMana;

        foreach(var anim in goldAnimators)
        {
            anim.Rebind();
        } 
        foreach(var anim in animators)
        {
            anim.Rebind();
        }
        animators.Clear(); 
        goldAnimators.Clear();
    }

}
