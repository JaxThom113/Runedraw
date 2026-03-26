using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Animations;
public class ManaUI : MonoBehaviour
{   
    

    public Transform ManaNodes;   
    public Animator ManaIcon;
    public GameObject ManaBorder;

    private Animator manaBorderAnimator;

    private const string GreyToGoldState = "GreyToGold";
    private const string GoldToFullState = "GoldToFull";
    private const string GoldToGreyState = "GoldToGrey";

    float animationLength = 0;
    private List<Animator> animators = new List<Animator>(); 
    private List<Animator> goldAnimators = new List<Animator>();
    private int manaAmount = 0; 

    private void Awake()
    {
        if (ManaBorder == null) return;
        // Prefer the animator on the object itself; fall back to children.
        manaBorderAnimator = ManaBorder.GetComponent<Animator>();
        if (manaBorderAnimator == null)
            manaBorderAnimator = ManaBorder.GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update 
    void OnEnable() { 
        
    }
    public void UpdateMana(int manaAmount)
    {
        this.manaAmount = manaAmount;
       
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
         SetManaBorderIncrease(true);
        yield return new WaitForSeconds(animationLength);   
        SetManaBorderIncrease(false);
       
        
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
        ManaIcon.Play("ManaIdle"); 
        SetManaBorderIncrease(true);
    
        
        
        for(int i = 0; i< animators.Count; i++)
        { 
            animators[i].Play("GoldToFull");
        } 
            yield return new WaitForSeconds(animationLength);  
        SetManaBorderIncrease(false);
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
    public void SetManaBorderIncrease(bool value)
    {
        if (ManaBorder == null) return;
        manaBorderAnimator.SetBool("increase", value);
    } 
    

    

}
