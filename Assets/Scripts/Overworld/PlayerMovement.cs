using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask whatStopsMovement;

    [Header("Movement Animation Settings")]
    public float jumpPower = 0.4f;
    public float jumpDuration = 0.2f;

     

   
    [SerializeField] KeyCode cameraRotateLeft = KeyCode.Q;
    [SerializeField] KeyCode cameraRotateRight = KeyCode.E;

    private bool isMoving = false;


    void Awake()
    {
      
    }

    void Start()
    {
       
    }

    void Update()
    { 
        if(CameraTransitionSystem.Instance.inBattleScene) return;
        if (Input.GetKeyDown(KeyCode.G))
            ResetMovePoint();

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f && !isMoving)
        {
           
            Vector3 dir = Vector3.zero;
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
                dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
            else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
                dir = new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);

            if (dir != Vector3.zero)
            {
                
                movePoint.parent = null;

                // if nothing in the position the player is trying to move to, then allow movement
                if (!Physics2D.OverlapCircle(movePoint.position + dir, 0.2f, whatStopsMovement))
                { 
                    SoundEffectSystem.Instance.PlayWalkSound();
                    movePoint.position += dir;

                    isMoving = true;

                    // DOTween movement & animation
                    transform.DOJump(
                        movePoint.position,   // target
                        jumpPower,            // height
                        1,                    // jumps
                        jumpDuration          // time
                    )
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => isMoving = false);
                }
            }
        }
    }

    public void ResetMovePoint()
    {
        // set the movepoint as child of the player when resetting
        movePoint.parent = transform;
        movePoint.localPosition = Vector3.zero;
    }
}
