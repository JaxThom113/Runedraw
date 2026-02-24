using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class PlayerMovement3 : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask whatStopsMovement;

    [Header("Movement Animation Settings")]
    public float jumpPower = 0.4f;
    public float jumpDuration = 0.2f;

    [Header("Camera")]
    [Tooltip("Must be a CHILD of the player (so rotating it doesn't move the player). Assign the camera rig or GameViewCamera.")]
    [SerializeField] Transform cameraRig;
    [Tooltip("Use separate keys so Horizontal (A/D) only moves; camera doesn't rotate when moving.")]
    [SerializeField] KeyCode cameraRotateLeft = KeyCode.Q;
    [SerializeField] KeyCode cameraRotateRight = KeyCode.E;

    private bool isMoving = false;
    private float cameraRotationX;   // only this changes with input
    private float cameraBaseY, cameraBaseZ;  // fixed, never touched

    void Awake()
    {
        if (cameraRig == null)
            cameraRig = transform.Find("GameViewCamera");
    }

    void Start()
    {
        if (cameraRig != null)
        {
            Vector3 e = cameraRig.localEulerAngles;
            cameraRotationX = e.x;
            cameraBaseY = e.y;
            cameraBaseZ = e.z;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            ResetMovePoint();

        // // Camera rotate: separate keys (Q/E) so Horizontal doesn't rotate camera
        // if (cameraRig != null)
        // {
        //     if (Input.GetKeyDown(cameraRotateLeft))
        //     {
        //         cameraRotationX -= 90f;
        //         cameraRig.localRotation = Quaternion.Euler(cameraRotationX, cameraBaseY, cameraBaseZ);
        //     }
        //     else if (Input.GetKeyDown(cameraRotateRight))
        //     {
        //         cameraRotationX += 90f;
        //         cameraRig.localRotation = Quaternion.Euler(cameraRotationX, cameraBaseY, cameraBaseZ);
        //     }
        // }

        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f && !isMoving)
        {
            // Movement: Horizontal = left/right, Vertical = up/down (no camera rotation here)
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

    public void ContinueUp()
    {
        // automatic upward movement used when transitioning between levels

    }

    public void TeleportToBottom()
    {
        // tp to bottom
        
    }
}
