using UnityEngine;
using TMPro;
using System.Collections.Generic; 
using DG.Tweening;
public class StatusUI : MonoBehaviour
{ 
  //Poison
  public GameObject Poison; 
  public TextMeshProUGUI poisonDuration; 
  public TextMeshProUGUI poisonStacks; 
  public GameObject PoisonIcon; 

  //Bleed
  public GameObject Bleed; 
  public TextMeshProUGUI bleedDuration; 
  public TextMeshProUGUI bleedStacks; 
  public GameObject BleedIcon; 

  //Vunerable
  public GameObject Vunerable;
  public TextMeshProUGUI vunerableDuration;
  public TextMeshProUGUI vunerableStacks;
  public GameObject VunerableIcon;

  //Stun
  public GameObject Stun;
  public TextMeshProUGUI stunDuration;
  public TextMeshProUGUI stunStacks;
  public GameObject StunIcon;

  public void SetPoisonVisible(bool visible)
  { 
    if (Poison == null) return; 

    Poison.SetActive(visible);
  }

  public void SetBleedVisible(bool visible)
  { 
    if (Bleed == null) return;
    Bleed.SetActive(visible);
  }

  public void SetVunerableVisible(bool visible)
  {
    if (Vunerable == null) return;
    Vunerable.SetActive(visible);
  }

  public void SetStunVisible(bool visible)
  {
    if (Stun == null) return;
    Stun.SetActive(visible);
  }

  public void UpdateBleed(int durationTicks, int stacks)
  { 
    if (bleedDuration != null) bleedDuration.text = durationTicks.ToString();
    if (bleedStacks != null) bleedStacks.text = stacks.ToString();
    SetBleedVisible(stacks > 0);
  }

  public void UpdateVunerable(int durationTicks, int stacks)
  {
    if (vunerableDuration != null) vunerableDuration.text = durationTicks.ToString();
    if (vunerableStacks != null) vunerableStacks.text = stacks.ToString();
    SetVunerableVisible(stacks > 0);
  }

  public void UpdateStun(int durationTicks, int stacks)
  {
    if (stunDuration != null) stunDuration.text = durationTicks.ToString();
    if (stunStacks != null) stunStacks.text = stacks.ToString();
    SetStunVisible(stacks > 0);
    // If the root Stun container was not wired in the Inspector, still toggle the TMP rows so something shows.
    if (Stun == null)
    {
      bool show = stacks > 0;
      if (stunDuration != null) stunDuration.gameObject.SetActive(show);
      if (stunStacks != null) stunStacks.gameObject.SetActive(show);
      if (StunIcon != null) StunIcon.SetActive(show);
    }
  }

  public void UpdateStatus(Dictionary<StatusEffect, int> map, Dictionary<StatusEffect, int> turnMap)
  {
    
  }

  public void RefreshUI(int poisonDurationTicks, int poisonStackCount, int bleedDurationTicks, int bleedStackCount)
  {
    UpdatePoison(poisonDurationTicks, poisonStackCount);
    UpdateBleed(bleedDurationTicks, bleedStackCount);
  }

  public void UpdatePoison(int durationTicks, int stacks)
  { 
    // durationTicks: turns remaining until poison triggers its next effect.
    // stacks: current poison stack count.
    if (poisonDuration != null) poisonDuration.text = durationTicks.ToString();
    if (poisonStacks != null) poisonStacks.text = stacks.ToString();
    SetPoisonVisible(stacks > 0);
  } 
  

  public void ScreenShake()
  {
    if (PoisonIcon == null) return;

    RectTransform rt = PoisonIcon.GetComponent<RectTransform>();
    if (rt == null) return;
    // Save current placement so we don't permanently move the icon.
    Vector2 originalAnchorMin = rt.anchorMin;
    Vector2 originalAnchorMax = rt.anchorMax;
    Vector2 originalAnchoredPos = rt.anchoredPosition;

    rt.DOKill(); // cancel any tweens on the icon

    // Center it, then shake.
    rt.anchorMin = new Vector2(0.5f, 0.5f);
    rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.anchoredPosition = Vector2.zero;

    Sequence seq = DOTween.Sequence();
    seq.Append(rt.DOShakeAnchorPos(0.5f, new Vector2(24f, 24f), 14, 90f, false, true));
    seq.AppendCallback(() =>
    {
      rt.anchorMin = originalAnchorMin;
      rt.anchorMax = originalAnchorMax;
      rt.anchoredPosition = originalAnchoredPos;
    });
  }

}
