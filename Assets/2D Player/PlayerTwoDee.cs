using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerTwoDee : MonoBehaviour {
    [SerializeField] private float speed;
    private PlayerActions actions;
    private Rigidbody2D rb;
    private BoxCollider2D box;
    private Vector2 moveInput;

    [SerializeField] private float jumpPower;
    [SerializeField] [Range(1f, 5f)] private float jumpFallGravity;

    [SerializeField] private float groundCheckHeight;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float disableGCTime;

    private Vector2 boxCenter;
    private Vector2 boxSize;
    private bool jumping;
    private float initGravityScale;
    private bool groundCheckEnable = true;
    private WaitForSeconds _w;

    private bool preJump;
    private float jumpTimer;


    void Awake() {
        actions = new PlayerActions();
        rb = GetComponent<Rigidbody2D>();
        initGravityScale = rb.gravityScale;
        box = GetComponent<BoxCollider2D>();
        _w = new WaitForSeconds(disableGCTime);
        actions.Player_Map.Jump.performed += Jump;
    }
    private void OnEnable() {
        actions.Player_Map.Enable();
    }
    private void OnDisable() {
        actions.Player_Map.Disable();
    }
    private void Jump(InputAction.CallbackContext context) {
        if (IsGrounded()) {
            rb.velocity += Vector2.up * jumpPower;
            jumping = true;
            preJump = true;
            jumpTimer = 0.2f;
            StartCoroutine(EnabeGroundCheckAfterJump());
        }
    }

    private void ManualJump() {
        rb.velocity += Vector2.up * jumpPower;
        jumping = true;
    }

    private bool IsGrounded() {
        boxCenter = new Vector2(box.bounds.center.x, box.bounds.center.y) + (Vector2.down * (box.bounds.extents.y + (groundCheckHeight / 2f)));
        boxSize = new Vector2(box.bounds.size.x, groundCheckHeight);
        var groundBox = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundMask);
        if (groundBox != null) {
            return true;
        }
        return false;
    }
    private IEnumerator EnabeGroundCheckAfterJump() {
        groundCheckEnable = false;
        yield return _w;
        groundCheckEnable = true;
    }

    private void HandleGravity() {
        if (groundCheckEnable && IsGrounded()) {
            jumping = false;
            if(jumpTimer > 0f)
            {
                ManualJump();
            }
        } 
        else if(jumping&&rb.velocity.y < 0f) {
            rb.gravityScale = initGravityScale * jumpFallGravity;
        } 
        else {
            rb.gravityScale = initGravityScale;
        }
    }
    void FixedUpdate() {
        moveInput = actions.Player_Map.Movement.ReadValue<Vector2>() * speed;
        rb.velocity = new Vector2(moveInput.x, rb.velocity.y);
        if(preJump == true)
        {
            jumpTimer -= Time.deltaTime;
        }
        HandleGravity();
    }
    private void OnDrawGizmos() {
        if (jumping) {
            Gizmos.color = Color.red;
        }
        else { 
            Gizmos.color = Color.green;
        }
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
