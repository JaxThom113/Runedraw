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
    public bool LootCard = false;
    public bool InventoryCard = false;
    //ALL TYPES MUST BE THE SAME
    // Start is called before the first frame update
    public void Setup(Card card)
    {  
        if(card == null) return; 
      
        this.card = card; 
        cardBorder.GetComponent<Image>().sprite = card.cardBorder; 
        cardIcon.GetComponent<Image>().sprite = card.cardIcon; 
        cardCostText.GetComponent<TextMeshProUGUI>().text = card.cardCost.ToString(); 
        cardDescriptionText.GetComponent<TextMeshProUGUI>().text = card.cardDescription; 
        //cardElement.GetComponent<Image>().sprite = card.cardElement;  
        cardTypeIcon.GetComponent<Image>().sprite = card.cardTypeIcon;
        cardElementIcon.GetComponent<Image>().sprite = card.cardElementIcon;
        //cardActions.GetComponent<Image>().sprite = card.cardActions;
        cardNameText.GetComponent<TextMeshProUGUI>().text = card.cardName;   
        //cardType = card.cardType; 
        cardTypeName.GetComponent<TextMeshProUGUI>().text = card.cardType.ToString();
    }
    void OnEnable()
    {
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
        wrapper.SetActive(false);
        
        transform.SetAsLastSibling();
        CardViewHoverSystem.Instance.Show(card, transform.position);
    } 
    public void OnPointerDown(PointerEventData eventData)
    { 
        if(LootCard){ 
            PlayerSystem.Instance.player.AddCardToDeck(card.data); 
            LevelSystem.Instance.LootView.SetActive(false);
            PlayerWinGA playerWinGA = new();
            ActionSystem.Instance.Perform(playerWinGA);
            return;
        }
        if(InventoryCard) return; 
        if (!Interactions.Instance.PlayerCanInteract()) return;
        Interactions.Instance.PlayerIsDragging = true;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
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
            Debug.LogError("Card is null");
            return;
        }
        if( transform.localPosition.y > 200f && ManaSystem.Instance.HasEnoughMana(card.cardCost)) 
        {  
            PlayCardGA playCardGA = new(card); 
            ActionSystem.Instance.Perform(playCardGA);  //action
        } 
        else
        {
            transform.DOMove(initialPosition, 0.2f);
            transform.DORotateQuaternion(initialRotation, 0.2f);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if(InventoryCard || LootCard) return;
       //if(tweening) return;
        if(!Interactions.Instance.PlayerCanHover()) return;
        wrapper.SetActive(true);
        transform.SetAsFirstSibling();
        CardViewHoverSystem.Instance.Hide();
    }
    
    // Note: OnMouseEnter/Exit only work for 3D/2D objects with colliders, not UI
}
