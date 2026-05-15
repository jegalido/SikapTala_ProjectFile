using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
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
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            if (DialogueManager.dialogueManagerInstance != null)
                DialogueManager.dialogueManagerInstance.OnAdvanceInput();
        }

        // Block movement input during dialogue or when controls disabled
        if (inDialogue || !controlsEnabled) return;

        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
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

        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
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