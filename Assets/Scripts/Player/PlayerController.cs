using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;

    [SerializeField] private float moveSpeed;

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
        HandleMovemnent();
    }



    private void HandleMovemnent()
    {
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }
}
