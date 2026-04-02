using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; 
using DG.Tweening;
public class ApplyCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
{  
    //All this script does is apply the card data to the UI. 
    //Data is handled in the Card class. 
    //Data originates from the CardSO scriptable object, 
    //and is then fed to the Card class, where it is finally applied here. 
    public Card card; 
    [SerializeField] public GameObject CardBorderEnter; 
    [SerializeField] public GameObject CardBorderExit; 
    [SerializeField] public GameObject cardBorder; 
    [SerializeField] public GameObject cardIcon;  
    [SerializeField] public GameObject cardCostText;  
    //[SerializeField] public GameObject cardTypeIcon;  
    [SerializeField] public GameObject cardTypeIcon; 
    [SerializeField] public GameObject cardTypeName; 
    [SerializeField] public GameObject cardElementIcon;  
    //[SerializeField] private GameObject cardActions;  
    [SerializeField] public GameObject cardDescriptionText; 
    //[SerializeField] private GameObject cardElement; 
    [SerializeField] public GameObject cardNameText;   
    [SerializeField] public GameObject wrapper; 

    private bool tweening = true;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool ultimateWindupActive;
    private TextMeshProUGUI costTextComponent;

    public const float UltimateWindupSeconds = 1.5f;
    public const float UltimateScale = 0.5f;
    public const float UltimateTweenDuration = 0.25f;

    public IEnumerator UltimateWindupRoutine()
    {
        if (transform == null) yield break; 
        transform.DOKill();
        transform.DOScale(Vector3.one * UltimateScale, UltimateTweenDuration).SetEase(Ease.OutQuad); 
        transform.DOMove(Vector3.zero + new Vector3(0, 1, 0), UltimateTweenDuration);
        yield return new WaitForSeconds(UltimateTweenDuration); 
        UISystem.Instance.TransformShake(this.transform);
        yield return new WaitForSeconds(UltimateWindupSeconds); 
        transform.DOMove(new Vector3(25, 1, 0), UltimateTweenDuration); 
        yield return new WaitForSeconds(UltimateTweenDuration);
    }
    public bool LootCard = false;
    public bool InventoryCard = false;
    public bool IsEnemyCard = false;
    public bool LootFromEnemy = false;
    //ALL TYPES MUST BE THE SAME
    // Start is called before the first frame update
    public void Setup(Card card)
    {  
        if(card == null) return; 
      
        this.card = card; 
        cardBorder.GetComponent<Image>().sprite = card.cardBorder; 
        cardIcon.GetComponent<Image>().sprite = card.cardIcon; 
        costTextComponent = cardCostText != null ? cardCostText.GetComponent<TextMeshProUGUI>() : null;
        RefreshManaCostText();
        RefreshDescriptionText();
        //cardElement.GetComponent<Image>().sprite = card.cardElement;  
        cardTypeIcon.GetComponent<Image>().sprite = card.cardTypeIcon;
        cardElementIcon.GetComponent<Image>().sprite = card.cardElementIcon;
        //cardActions.GetComponent<Image>().sprite = card.cardActions;
        cardNameText.GetComponent<TextMeshProUGUI>().text = card.cardName;   
        //cardType = card.cardType; 
        cardTypeName.GetComponent<TextMeshProUGUI>().text = card.cardType.ToString();
    }

    public void RefreshManaCostText()
    {
        if (card == null || costTextComponent == null) return;
        int displayedCost = card.cardCost;
        if (!InventoryCard && !LootCard && ManaSystem.Instance != null)
            displayedCost = ManaSystem.Instance.GetModifiedManaCost(card.cardCost);
        costTextComponent.text = displayedCost.ToString();
    }
    public void RefreshDescriptionText()
    {
        int playerVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(true) : 0;
        int enemyVunerableBonus = VunerableSystem.Instance != null ? VunerableSystem.Instance.GetTotalAdditionalDamage(false) : 0;
        RefreshDescriptionText(playerVunerableBonus, enemyVunerableBonus);
    }

    public void RefreshDescriptionText(int playerVunerableBonus, int enemyVunerableBonus)
    {
        if (card == null || cardDescriptionText == null) return;
        string description = card.data != null ? card.data.cardDescription : "";
        int displayAdditionalDamage = IsEnemyCard ? playerVunerableBonus : enemyVunerableBonus;
        foreach (Effect effect in card.effects)
        {
            effect.isPlayer = !IsEnemyCard;
            effect.displayAdditionalDamage = displayAdditionalDamage;
            if (!string.IsNullOrWhiteSpace(description))
                description += "\n";
            description += effect.GetDescription();
        }
        cardDescriptionText.GetComponent<TextMeshProUGUI>().text = description;
    }
    void OnEnable()
    {
        SetHoverBorderState(false);
        StartCoroutine(TweeningCooldown());
    }
    private IEnumerator TweeningCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        //tweening = false;
    }
    void Start()
    {
        // Removed test call - use ManaSystem.Instance when needed (e.g. OnPointerUp)
    }

    // Update is called once per frame
    void Update()
    {
        
    } 
    // UI Event System - works for UI objects (Image, Button, etc.)
    public void OnPointerEnter(PointerEventData eventData)
    {  
        if(InventoryCard || LootCard) return;
        //if(tweening) return;
        if(!Interactions.Instance.PlayerCanHover()) return;
        SetHoverBorderState(true);
        wrapper.SetActive(false);
        
        transform.SetAsLastSibling();
        CardViewHoverSystem.Instance.Show(card, transform.position);
    } 
    public void OnPointerDown(PointerEventData eventData)
    { 
        if(LootCard){
            SoundEffectSystem.Instance.PlayButtonClickSound();
            PlayerSystem.Instance.player.AddCardToDeck(card.data); 
            ActionSystem.Instance.Perform(new LootCardPickupGA(LootFromEnemy));
            return;
        }
        if(InventoryCard) return; 
        if (!Interactions.Instance.PlayerCanInteract()) return;
        Interactions.Instance.PlayerIsDragging = true;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        SetHoverBorderState(false);
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
        transform.rotation = Quaternion.identity;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(InventoryCard || LootCard) return;
        if (!Interactions.Instance.PlayerIsDragging) return;

        Vector3 worldPoint; 
        wrapper.SetActive(true);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)transform.parent, 
            eventData.position, 
            eventData.pressEventCamera, 
            out worldPoint))
        {
            transform.position = new Vector3(worldPoint.x, worldPoint.y, worldPoint.z - 0.1f);
        }
    } 

    public void OnPointerUp(PointerEventData eventData)
    {
        if(InventoryCard || LootCard) return;
        
        if (card == null ) 
        {
            Interactions.Instance.PlayerIsDragging = false;
            return;
        }
        if( transform.localPosition.y > 200f && ManaSystem.Instance.HasEnoughMana(card.cardCost)) 
        {   
            Interactions.Instance.PlayerIsDragging = false;
            if (card.IsUltimate)
            {
                if (!ultimateWindupActive)
                    StartCoroutine(PlayCardAfterUltimateWindup());
                return;
            }
            PlayCardGA playCardGA = new(card); 
            ActionSystem.Instance.Perform(playCardGA);  //action
        } 
        else
        {
            Interactions.Instance.PlayerIsDragging = false;
            if (HandView.Instance != null)
                StartCoroutine(ReturnCardToHandSpline());
        }
    }

    IEnumerator PlayCardAfterUltimateWindup()
    {
        ultimateWindupActive = true;
        yield return UltimateWindupRoutine();
        ultimateWindupActive = false;
        if (card == null) yield break;
        PlayCardGA playCardGA = new(card);
        ActionSystem.Instance.Perform(playCardGA);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if(InventoryCard || LootCard) return;
       //if(tweening) return;
        if(!Interactions.Instance.PlayerCanHover()) return;
        SetHoverBorderState(false);
        wrapper.SetActive(true);
        transform.SetAsFirstSibling();
        CardViewHoverSystem.Instance.Hide();
    }

    private void SetHoverBorderState(bool hoverActive)
    {
        if (CardBorderEnter != null)
            CardBorderEnter.SetActive(!hoverActive);

        if (CardBorderExit != null)
            CardBorderExit.SetActive(hoverActive);
    }

    private IEnumerator ReturnCardToHandSpline()
    {
        SetHoverBorderState(false);
        wrapper.SetActive(true);
        transform.SetAsFirstSibling();
        CardViewHoverSystem.Instance.Hide();

        if (Interactions.Instance != null)
            Interactions.Instance.PlayerHoverLocked = true;

        HandView.Instance.RefreshHandLayout();

        yield return new WaitForSeconds(HandView.Instance.duration);

        if (Interactions.Instance != null)
            Interactions.Instance.PlayerHoverLocked = false;
    }
    
    // Note: OnMouseEnter/Exit only work for 3D/2D objects with colliders, not UI
}
