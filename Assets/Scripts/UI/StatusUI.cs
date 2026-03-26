using UnityEngine;
using TMPro;
using System.Collections.Generic; 
using DG.Tweening;
public class StatusUI : MonoBehaviour
{
  public GameObject Poison; 
  public TextMeshProUGUI poisonDuration; 
  public TextMeshProUGUI poisonStacks; 
  public GameObject PoisonIcon;

  public void SetPoisonVisible(bool visible)
  { 
    Debug.Log("SetPoisonVisible: " + visible);
    if (Poison == null) { 
        Debug.LogError("Poison GameObject is not assigned in StatusUI");
        return;
    } 

    Poison.SetActive(visible);
  }

  public void UpdatePoison(int durationTicks, int stacks)
  { 
    Debug.Log("UpdatePoison: durationTicks: " + durationTicks + " stacks: " + stacks);
    // durationTicks: turns remaining until poison triggers its next effect.
    // stacks: current poison stack count.
    if (poisonDuration != null) poisonDuration.text = durationTicks.ToString();
    if (poisonStacks != null) poisonStacks.text = stacks.ToString();
    SetPoisonVisible(stacks > 0);
  }

  public void UpdateStatus(Dictionary<StatusEffect, int> map, Dictionary<StatusEffect, int> turnMap)
  {
    Debug.Log("UpdateStatus: map: " + map.Count + " turnMap: " + turnMap.Count);
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
