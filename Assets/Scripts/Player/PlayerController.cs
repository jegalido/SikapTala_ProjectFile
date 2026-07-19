using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runSpeedMultiplier = 1.5f;
    [SerializeField] private float jumpForce;
    [Header("Collision detection")]
    [SerializeField] private float groundCheck;
    [SerializeField] private LayerMask thisIsGround;
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip[] footstepSFX;
    private AudioSource audioSource;
    private bool isGrounded;
    public float xInput;
    public bool isRunning;
    private bool facingRight = true;
    public bool inDialogue = false;
    private bool controlsEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    void Start() { }
    void Update()
    {
        HandleCollision();
        HandleInput();
        HandleMovemnent();
        HandleFlip();
        HandleAnimation();
    }
    private void HandleInput()
    {
        Gamepad pad = Gamepad.current;

        // Advance dialogue: E, left click, or Square (interact)
        bool interactPressed = Input.GetKeyDown(KeyCode.E)
    || Input.GetMouseButtonDown(0)
    || (pad != null && pad.buttonWest.wasPressedThisFrame)
    || (pad != null && pad.buttonEast.wasPressedThisFrame);

        if (interactPressed)
        {
            if (DialogueManager.dialogueManagerInstance != null)
                DialogueManager.dialogueManagerInstance.OnAdvanceInput();
        }

        // Block movement input during dialogue or when controls disabled
        if (inDialogue || !controlsEnabled) return;

        xInput = Input.GetAxisRaw("Horizontal");
        if (pad != null && Mathf.Abs(pad.leftStick.x.ReadValue()) > 0.1f)
            xInput = pad.leftStick.x.ReadValue();

        // Run: Left Shift or L2
        isRunning = Input.GetKey(KeyCode.LeftShift)
            || (pad != null && pad.rightTrigger.ReadValue() > 0.1f);

        // Jump: Space or X (Cross/South)
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space)
            || (pad != null && pad.buttonSouth.wasPressedThisFrame);

        if (jumpPressed && isGrounded)
            HandleJump();
    }
    private void HandleJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (jumpSFX != null)
            audioSource.PlayOneShot(jumpSFX);
    }
    public void PlayFootstep()
    {
        if (!isGrounded) return;
        if (Mathf.Abs(xInput) < 0.1f) return;
        if (footstepSFX.Length == 0) return;
        if (inDialogue) return;
        if (!controlsEnabled) return;
        int randomIndex = Random.Range(0, footstepSFX.Length);
        audioSource.PlayOneShot(footstepSFX[randomIndex]);
    }
    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheck, thisIsGround);
    }
    private void HandleAnimation()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }
    private void HandleMovemnent()
    {
        if (inDialogue || !controlsEnabled)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        float speed = moveSpeed * (isRunning ? runSpeedMultiplier : 1f);
        rb.linearVelocity = new Vector2(xInput * speed, rb.linearVelocity.y);
    }
    private void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && !facingRight || rb.linearVelocity.x < 0 && facingRight)
            Flip();
    }
    private void Flip()
    {
        transform.Rotate(0f, 180f, 0f);
        facingRight = !facingRight;
    }
    public void EnableControl()
    {
        controlsEnabled = true;
        inDialogue = false;
        Debug.Log("PlayerController: Controls enabled.");
    }
    public void DisableControl()
    {
        controlsEnabled = false;
        xInput = 0f;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        Debug.Log("PlayerController: Controls disabled.");
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheck));
    }
}