using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 9f;
    public float groundCheckDistance = 0.12f;

    [Header("Animation")]
    public float walkAnimationSpeed = 0.6f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D playerCollider;

    private float moveInput;
    private bool isRunning;
    private int facing = 1;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsAttackingParam = Animator.StringToHash("IsAttacking");
    private static readonly int IsJumpingParam = Animator.StringToHash("IsJumping");
    private static readonly int IsDeadParam = Animator.StringToHash("IsDead");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        rb.gravityScale = 1f;
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (Input.GetKeyDown(KeyCode.J) && !IsAttacking())
            animator.SetBool(IsAttackingParam, true);

        if (IsAttacking() && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            animator.SetBool(IsAttackingParam, false);

        UpdateFacing();
    }

    private void FixedUpdate()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    private void LateUpdate()
    {
        bool grounded = IsGrounded();
        bool moving = Mathf.Abs(moveInput) > 0.01f;

        animator.SetFloat(SpeedParam, moving ? Mathf.Abs(moveInput) : 0f);
        animator.SetBool(IsJumpingParam, !grounded);
        animator.SetBool(IsDeadParam, false);

        animator.speed = moving && grounded && !IsAttacking() && !isRunning ? walkAnimationSpeed : 1f;
    }

    private void UpdateFacing()
    {
        if (!IsAttacking())
        {
            if (moveInput > 0) facing = 1;
            else if (moveInput < 0) facing = -1;
        }

        spriteRenderer.flipX = facing < 0;
    }

    private bool IsAttacking()
    {
        return animator.GetBool(IsAttackingParam);
    }

    private bool IsGrounded()
    {
        if (playerCollider == null)
            return false;

        float feetY = playerCollider.bounds.min.y;
        Vector2 origin = new Vector2(transform.position.x, feetY + 0.05f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, groundCheckDistance + 0.05f);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject)
                return true;
        }

        return false;
    }
}
