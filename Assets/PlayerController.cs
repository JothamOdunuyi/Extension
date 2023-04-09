using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    // Movement bools
    private bool isGrounded;
    private bool isJumping;
    
    // Components
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    
    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float xInput = Input.GetAxis("Horizontal");
        Vector2 moveDir = new Vector2(xInput, 0f).normalized;
        rb.velocity = moveDir * moveSpeed;
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            rb.velocity = Vector2.up * jumpForce;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}