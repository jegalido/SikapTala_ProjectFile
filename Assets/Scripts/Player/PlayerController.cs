using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;


    [Header("Collision detection")]
    [SerializeField] private float groundCheck;
    [SerializeField] private LayerMask thisIsGround;
    private bool isGrounded;


    
    public float xInput; 
    


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheck, thisIsGround);


        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        HandleMovemnent();

    }



    private void HandleMovemnent()
    {
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheck) );
    }
}
