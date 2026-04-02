using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InteractableCollision : MonoBehaviour
{ 
    [SerializeField] private Material material; 
    [SerializeField] private float fadeInDuration = 3f;
    [SerializeField] private float fadeOutPauseDuration = 0.2f;
    private static readonly Vector3 InteractViewPosition = new(0f, -1.5f, -1.0f);
    private static readonly Vector3 DefaultViewPosition = new(0f, 0f, -1.0f);

    private PlayerMovement playerMovement;
    private Collider interactableCollider;
    private bool interactionStarted;

    private void Awake()
    {
        interactableCollider = GetComponent<Collider>();
    }

    private void OnDisable()
    {
        ResetMaterial();
    }

    private void OnDestroy()
    {
        ResetMaterial();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || interactionStarted)
            return;

        interactionStarted = true;
        if (interactableCollider != null)
            interactableCollider.enabled = false;

        playerMovement = other.GetComponent<PlayerMovement>();

        LevelSystem.Instance?.BeginLootSelection(gameObject);
        ShowCurrentInteractableVisual();
        FadeIn();
        EnterInteractableView();

        StartCoroutine(LootCard());
    }

    public void FadeIn()
    {
        if (material == null)
            return;

        material.SetFloat("_FadeIn", 0.4f); 

        DOTween.To( 
            () => material.GetFloat("_FadeIn"), 
            (float x) => material.SetFloat("_FadeIn", x), 
            -0.2f, 
            fadeInDuration
        ).SetEase(Ease.InOutSine); 
    }

    public void FadeOut()
    {
        if (material == null)
            return;

        material.SetFloat("_FadeIn", -0.2f); 

        DOTween.To( 
            () => material.GetFloat("_FadeIn"), 
            (float x) => material.SetFloat("_FadeIn", x), 
            0.4f, 
            fadeInDuration
        ).SetEase(Ease.InOutSine);
    }

    public IEnumerator LootCard()
    {
        yield return new WaitForSeconds(fadeInDuration);
        ActionSystem.Instance.Perform(new LootCardGA(3));

        yield return new WaitUntil(() => LevelSystem.Instance == null || LevelSystem.Instance.LootSelectionCompleted);

        FadeOut();
        yield return new WaitForSeconds(fadeInDuration);
      

        yield return RestorePlayerState();
        ShowAllInteractableVisuals();
        Destroy(gameObject);
    }

    private void ShowCurrentInteractableVisual()
    {
        GameObject interactableContainer = GameObject.Find("InteractableContainer");
        if (interactableContainer == null)
            return;

        string interactableName = gameObject.name;
        foreach (Transform child in interactableContainer.transform)
        {
            child.gameObject.SetActive(child.name == interactableName);
        }
    }

    private void ShowAllInteractableVisuals()
    {
        GameObject interactableContainer = GameObject.Find("InteractableContainer");
        if (interactableContainer == null)
            return;

        foreach (Transform child in interactableContainer.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void EnterInteractableView()
    {
        if (playerMovement == null)
            return;

        playerMovement.enabled = false;
        playerMovement.playerViewContainer.SetActive(false);

        if (playerMovement.viewTarget != null)
        {
            playerMovement.viewTarget.DOKill();
            playerMovement.viewTarget.DOLocalMove(InteractViewPosition, 0.5f);
        }
    }

    private IEnumerator RestorePlayerState()
    {
        if (playerMovement == null)
            yield break;

        if (playerMovement.viewTarget != null)
        {
            playerMovement.viewTarget.DOKill();
            Tween returnTween = playerMovement.viewTarget.DOLocalMove(DefaultViewPosition, 0.5f);
            yield return returnTween.WaitForCompletion();
        }

        playerMovement.playerViewContainer.SetActive(true);
        playerMovement.ResetMovePoint();
        playerMovement.enabled = true;
    }

    private void ResetMaterial()
    {
        if (material != null)
            material.SetFloat("_FadeIn", 0.4f);
    }
}
