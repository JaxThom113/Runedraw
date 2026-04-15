using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ManaUI : MonoBehaviour
{   
    

    public Transform ManaNodes;   
    public Animator ManaIcon;
    public GameObject ManaBorder;

    private Animator manaBorderAnimator;

    float animationLength = 0;
    private List<Animator> animators = new List<Animator>(); 
    private List<Animator> goldAnimators = new List<Animator>();
    private int manaAmount = 0; 

    private Coroutine visualRoutine;

    private void Awake()
    {
        if (ManaBorder == null) return;
        // Prefer the animator on the object itself; fall back to children.
        manaBorderAnimator = ManaBorder.GetComponent<Animator>();
        if (manaBorderAnimator == null)
            manaBorderAnimator = ManaBorder.GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        CancelRunningVisualRoutine();
        HardResetAllNodeAnimators();
        animators.Clear();
        goldAnimators.Clear();
        if (ManaIcon != null)
            ManaIcon.Play("ManaIdle");
        SetManaBorderIncrease(false);
    }


    public void CancelRunningVisualRoutine()
    {
        if (visualRoutine != null)
        {
            StopCoroutine(visualRoutine);
            visualRoutine = null;
        }
    }

    private IEnumerator VisualRoutineWrapper(IEnumerator routine)
    {
        try
        {
            yield return routine;
        }
        finally
        {
            visualRoutine = null;
        }
    }


    private IEnumerable<Animator> EnumerateNodeAnimators()
    {
        if (ManaNodes == null)
            yield break;
        for (int i = 0; i < ManaNodes.childCount; i++)
        {
            Transform t = ManaNodes.GetChild(i);
            Animator a = t.GetComponent<Animator>();
            if (a == null)
                a = t.GetComponentInChildren<Animator>(true);
            if (a != null)
                yield return a;
        }
    }


    private void HardResetAllNodeAnimators()
    {
        foreach (Animator anim in EnumerateNodeAnimators())
        {
            if (anim == null)
                continue;
            anim.speed = 1f;
            anim.Rebind();
            anim.Update(0f);
        }
    }

    public void UpdateMana(int manaAmount)
    {
        this.manaAmount = manaAmount;
       
    } 
     
    public IEnumerator WaitForSpendMana(int manaAmount)
    {
        CancelRunningVisualRoutine();
        visualRoutine = StartCoroutine(VisualRoutineWrapper(CoSpendMana(manaAmount)));
        yield return visualRoutine;
    }

    private IEnumerator CoSpendMana(int manaAmount)
    {  
        if (ManaIcon != null)
            ManaIcon.Play("ManaIdle"); 
        for (int i = 0; i < manaAmount; i++)
        {
            animators.Last().Play("FullToGold"); 
            goldAnimators.Add(animators.Last());
            animators.RemoveAt(animators.Count - 1);
        } 
         SetManaBorderIncrease(true);
        yield return new WaitForSeconds(animationLength);   
        SetManaBorderIncrease(false);
    }
     
    
    public IEnumerator WaitForStartRound()
    {
        CancelRunningVisualRoutine();
        visualRoutine = StartCoroutine(VisualRoutineWrapper(CoStartRound()));
        yield return visualRoutine;
    }


    public void PlayStartRoundFireAndForget()
    {
        CancelRunningVisualRoutine();
        visualRoutine = StartCoroutine(VisualRoutineWrapper(CoStartRound()));
    }

    private IEnumerator CoStartRound() 
    {  
        animators.Clear();
        goldAnimators.Clear();

        if (ManaNodes == null)
            yield break;

        HardResetAllNodeAnimators();

        for (int i = 0; i < ManaNodes.childCount; i++)
        {
            Animator anim = ManaNodes.GetChild(i).GetComponent<Animator>();
            if (anim == null)
                anim = ManaNodes.GetChild(i).GetComponentInChildren<Animator>(true);
            if (anim == null)
                continue;
            if (i < manaAmount)
            {
                anim.Play("GreyToGold");
                animators.Add(anim);
                animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
            }
        }
        if (ManaIcon != null)
            ManaIcon.Play("ManaIdle"); 
        SetManaBorderIncrease(true);

        for (int i = 0; i < animators.Count; i++)
            animators[i].Play("GoldToFull");

        yield return new WaitForSeconds(animationLength);  
        SetManaBorderIncrease(false);
    } 

    public void ResetMana(int maxMana)
    {
        CancelRunningVisualRoutine();

        this.manaAmount = maxMana;

        HardResetAllNodeAnimators();
        animators.Clear(); 
        goldAnimators.Clear();
        if (ManaIcon != null)
            ManaIcon.Play("ManaIdle");
        SetManaBorderIncrease(false);
    } 
    public void SetManaBorderIncrease(bool value)
    {
        if (ManaBorder == null || manaBorderAnimator == null) return;
        manaBorderAnimator.SetBool("increase", value);
    } 
    

    

}
