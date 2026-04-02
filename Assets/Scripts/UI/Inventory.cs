using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI;
using TMPro;
public class Inventory : Singleton<Inventory>
{   
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;  
    [SerializeField] private GameObject inventoryContainer;  
    [SerializeField] private TextMeshProUGUI cardCountText; 
    [SerializeField] private Scrollbar scrollbar;
    private Canvas canvas;  
    private float containerH; 
    private float containerW;  
    private float cardWidth; 
    private float cardHeight;  
    private int totalRows;
    private float totalContentHeight;

    [Header("Settings")]
    public List<CardSO> cards = new List<CardSO>();  
    [SerializeField] private float cardScale = 0.3f;
    [SerializeField] private int cardsPerRow = 5;  
    [SerializeField] private float padding = 90; 
    [SerializeField] private float lineSpacing = 200;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 300f;
    private float currentScrollY = 0f;
    private float minScrollY = 0f;
    private float maxScrollY = 0f;
    private RectTransform containerRectTransform;
    private List<Vector2> cardBasePositions = new List<Vector2>(); // Store original card positions
    private bool displayed = false;
    public void AddCard(CardSO card) => cards.Add(card); 
    public int GetCardCount() => cards.Count; 
    public void Setup(List<CardSO> cards)
    {
        this.cards = cards; 
        
    }
    public List<CardSO> GetCards() => cards;
    public void ToggleCards() {
        AudioSystem.Instance.PlaySFX("click");
        if(displayed) { 
            Setup(PlayerSystem.Instance.player.playerDeck);
            HideCards();
        } else {
            DisplayCards();
        }
    } 
    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
       
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<LootCardPickupGA>(LootCardPickupPostReaction, ReactionTiming.POST);
        
    }
    public void LootCardPickupPostReaction(LootCardPickupGA lootCardPickupGA)
    {
        
        Setup(PlayerSystem.Instance.player.playerDeck); 
    }
   
    private void HideCards() { 
        displayed = false; 
        inventoryContainer.transform.parent.gameObject.SetActive(false);
        ClearDisplayedCards();
        cardBasePositions.Clear();
    }
    private void DisplayCards() { 
        displayed = true;    
        
        inventoryContainer.transform.parent.gameObject.SetActive(true);
        ClearDisplayedCards();
        cardBasePositions.Clear();

        
         containerW = inventoryContainer.GetComponent<RectTransform>().rect.width; 
         containerH = inventoryContainer.GetComponent<RectTransform>().rect.height;   

        RectTransform cardRectTransform = cardPrefab.GetComponent<RectTransform>();
        cardWidth = cardRectTransform.rect.width*cardRectTransform.localScale.x; 
        cardHeight = cardRectTransform.rect.height*cardRectTransform.localScale.y; 

        float cardsWidth = cardWidth * cardsPerRow; 
        float remainingWidth = containerW - cardsWidth - (padding*2);  
        float spacingX = cardsPerRow > 1 ? remainingWidth / (cardsPerRow - 1) : 0f;  
      
        
        for (int i = 0; i < cards.Count; i++) {  
             CardSO card = cards[i]; 

            int row = i / cardsPerRow; 
            int col = i % cardsPerRow;           
           

            float x = padding + col * (cardWidth + spacingX); 
            float y = -lineSpacing/2 - row * (cardHeight + lineSpacing);
       
            Vector3 cardPosition = new Vector3(x, y, 0);
            
            // Store base position for scrolling
            cardBasePositions.Add(new Vector2(x, y));
            
            GameObject cardObject = Instantiate(cardPrefab, cardPosition, Quaternion.identity);  
            cardObject.transform.SetParent(inventoryContainer.transform);  
            RectTransform rectTransform = cardObject.GetComponent<RectTransform>(); 
            if(rectTransform != null) {
                rectTransform.anchorMin = new Vector2(0,1); 
                rectTransform.anchorMax = new Vector2(0,1);  
                rectTransform.pivot = new Vector2(0,1);  
                rectTransform.anchoredPosition = cardPosition;
            } 
            else{ 
                    cardObject.transform.localPosition = cardPosition;
            }
            Card newCard = new Card(card);  
            ApplyCard applyCard = cardObject.GetComponent<ApplyCard>();
            applyCard.InventoryCard = true;
            applyCard.Setup(newCard); 
            applyCard.transform.localScale = new Vector3(cardScale, cardScale, cardScale);
            // Debug.Log("Card added: " + card.cardName);
        }
        
        // Calculate scroll limits based on content height (use Ceil so 6 cards with 5 per row = 2 rows)
        totalRows = cards.Count == 0 ? 0 : Mathf.CeilToInt((float)cards.Count / cardsPerRow);
        totalContentHeight = totalRows == 0 ? 0f : (totalRows - 1) * (cardHeight + lineSpacing) + cardHeight + lineSpacing;
        float scrollRange = Mathf.Max(0f, totalContentHeight - containerH + padding);
        maxScrollY = scrollRange;  // positive offset: 0 = top, maxScrollY = bottom
        minScrollY = 0f;
        currentScrollY = 0f; // Reset scroll when cards are displayed
        if (scrollbar != null)
        {
            scrollbar.size = scrollRange > 0.01f ? Mathf.Clamp01(containerH / totalContentHeight) : 1f;
        }
        ApplyScroll();

    }
    
    private void ClearDisplayedCards()
    {
        // Destroy all card children
        for (int i = inventoryContainer.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(inventoryContainer.transform.GetChild(i).gameObject);
        }
    }
    
    private void ApplyScroll()
    {
        // Move each card based on scroll offset
        for (int i = 0; i < inventoryContainer.transform.childCount && i < cardBasePositions.Count; i++)
        {
            Transform cardTransform = inventoryContainer.transform.GetChild(i);
            RectTransform rectTransform = cardTransform.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 basePos = cardBasePositions[i];
                rectTransform.anchoredPosition = new Vector2(basePos.x, basePos.y + currentScrollY);
            }
            else
            {
                Vector2 basePos = cardBasePositions[i];
                cardTransform.localPosition = new Vector3(basePos.x, basePos.y + currentScrollY, 0);
            }
        }
        // Scrollbar: value 0 = top of list, value 1 = bottom — value going UP moves cards UP and reveals hidden lower cards
        if (scrollbar != null)
        {
            float range = maxScrollY - minScrollY;
            if (range > 0.01f)
                scrollbar.SetValueWithoutNotify(Mathf.Clamp01(currentScrollY / range));
            else
                scrollbar.SetValueWithoutNotify(0f);
        }
    }  
    //Method to determine the max and min scroll values
    
    private void OnScrollbarValueChanged(float value) { 
        float range = maxScrollY - minScrollY;  
        if(range > 0.01f) { 
            // value 0 = top, value 1 = bottom — value going UP = currentScrollY up = cards move UP = reveal the 5 hidden cards
            float newScrollY = value * range; 
            if(Mathf.Abs(newScrollY - currentScrollY) > 0.01f) {
                currentScrollY = newScrollY;   
                ApplyScroll();
            }
        } 
    }
    void Start() {  
        canvas = inventoryContainer.GetComponentInParent<Canvas>(); 
        containerRectTransform = inventoryContainer.GetComponent<RectTransform>(); 
        scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
        
    }  
    
    void Update()
    {

        float scrollDelta = -Input.mouseScrollDelta.y;
        
        if (scrollDelta != 0 && inventoryContainer != null && displayed)
        {
            currentScrollY += scrollDelta * scrollSpeed * Time.deltaTime;
            currentScrollY = Mathf.Clamp(currentScrollY, minScrollY, maxScrollY);
            ApplyScroll();
        }
        UpdateCardCountText();
    }
    private void UpdateCardCountText() {
        cardCountText.text = cards.Count.ToString();
    }
   
}
