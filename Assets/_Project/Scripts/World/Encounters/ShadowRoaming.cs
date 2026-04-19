using UnityEngine;
using UnityEngine.SceneManagement;

public class ShadowRoaming : MonoBehaviour
{
    [Header("Dữ liệu & Quản lý")]
    public ThingData myData; // ScriptableObject chứa stats
    [HideInInspector] public GrassZoneManager parentManager; // Spawner quản lý con này

    [Header("Chỉ số di chuyển")]
    public float walkSpeed = 1.5f;
    public float chaseSpeed = 3.0f;
    public float detectionRange = 3.5f;

    [Header("Tính cách")]
    public bool isFleeingType = false;
    public bool isAggressiveType = false;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 moveDirection;
    private float timer;
    private float currentSpeed;
    private bool isInteractingWithPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        currentSpeed = walkSpeed;

        // Nếu chưa thiết lập tính cách trong Inspector, tự ngẫu nhiên
        if (!isFleeingType && !isAggressiveType)
        {
            if (Random.value > 0.2f) isFleeingType = true; // 80% là nhát
            else isAggressiveType = true; // 20% hung dữ
        }

        PickNewDirection();
    }

    void Update()
    {
        if (player != null) CheckForPlayer();

        if (!isInteractingWithPlayer)
        {
            timer -= Time.deltaTime;
            if (timer <= 0) PickNewDirection();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * currentSpeed;
    }

    void CheckForPlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            isInteractingWithPlayer = true;
            currentSpeed = chaseSpeed;

            if (isFleeingType)
                moveDirection = (transform.position - player.position).normalized; // Chạy xa khỏi người chơi
            else if (isAggressiveType)
                moveDirection = (player.position - transform.position).normalized; // Lao vào người chơi
        }
        else if (isInteractingWithPlayer)
        {
            isInteractingWithPlayer = false;
            currentSpeed = walkSpeed;
            PickNewDirection();
        }
    }

    void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        timer = Random.Range(1.5f, 4f);
    }

    // Được gọi khi Thing bị bắt hoặc chuyển cảnh chiến đấu
    public void OnCaptured()
    {
        if (parentManager != null)
        {
            parentManager.OnThingRemoved(); // Báo về Manager để trừ biến đếm (currentActiveThings)
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Đụng tường/Ranh giới bụi cỏ
        if (collision.gameObject.layer == LayerMask.NameToLayer("MonsterBoundary"))
        {
            PickNewDirection();
        }

        // 2. Đụng người chơi (Nếu hung dữ -> Vào trận đấu ngay)
        if (isAggressiveType && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=red>PHỤC KÍCH! Thing đã tấn công bạn!</color>");

            // Nạp dữ liệu và chuyển cảnh
            RuntimeGameState.CurrentEnemy = myData;
            OnCaptured();
            SceneManager.LoadScene("BattleScene");
        }
    }
}