using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private Vector2 lastDirection = new Vector2(0, -1); // Mặc định nhìn xuống dưới

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Đảm bảo Rigidbody2D được thiết lập đúng để tránh rung
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = 0; // Đảm bảo không bị rơi
    }

    void Update()
    {
        // 1. Lấy input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            movement = movement.normalized;
            lastDirection = movement; // Lưu lại hướng cuối cùng để Idle đúng hướng

            // Cập nhật hướng cho Animator
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
        }

        // 2. Cập nhật Speed để chuyển giữa Idle và Walk
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // 3. Xử lý Flip (Chỉ dùng nếu bạn không có animation hướng Trái riêng)
        if (movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;
    }

    void FixedUpdate()
    {
        // Di chuyển bằng Rigidbody mượt hơn với FixedDeltaTime
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}