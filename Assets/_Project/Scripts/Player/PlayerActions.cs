using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    public enum HuntingMode { Ball, Gun }
    [Header("Chế độ hiện tại")]
    public HuntingMode currentMode = HuntingMode.Ball;

    [Header("Cấu hình hồng tâm")]
    public GameObject reticlePrefab;
    public float maxRange = 3f; // Tầm xa 3 ô

    [Header("Prefabs Đạn/Bóng")]
    public GameObject ballPrefab;
    public GameObject bulletPrefab;

    private GameObject activeReticle;
    private SpriteRenderer reticleRenderer;
    private Camera cam;

    void Start()
    {
        cam = Camera.main; // Đảm bảo Camera của bạn có Tag là "MainCamera"

        if (reticlePrefab != null)
        {
            activeReticle = Instantiate(reticlePrefab);
            reticleRenderer = activeReticle.GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        // 1. Đổi chế độ
        if (Input.GetKeyDown(KeyCode.Q)) currentMode = HuntingMode.Ball;
        if (Input.GetKeyDown(KeyCode.E)) currentMode = HuntingMode.Gun;

        // 2. Cập nhật vị trí hồng tâm theo chuột
        UpdateReticlePosition();

        // 3. Thực hiện hành động khi nhấn Chuột Trái
        if (Input.GetMouseButtonDown(0))
        {
            if (currentMode == HuntingMode.Ball) ExecuteAction(ballPrefab, true);
            else ExecuteAction(bulletPrefab, false);
        }
    }

    void UpdateReticlePosition()
    {
        if (activeReticle == null || cam == null) return;

        // Lấy vị trí chuột từ màn hình và chuyển sang tọa độ thế giới (World Space)
        // QUAN TRỌNG: Cần gán Z bằng khoảng cách từ Camera tới mặt phẳng 2D (thường là 10)
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(cam.transform.position.z);

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0; // Đưa về mặt phẳng 2D

        // Tính toán khoảng cách
        float distance = Vector2.Distance(transform.position, mouseWorldPos);

        if (distance > maxRange)
        {
            // Nếu chuột ở ngoài tầm, hồng tâm dừng lại ở biên vòng tròn 3 ô
            Vector2 direction = (mouseWorldPos - transform.position).normalized;
            activeReticle.transform.position = (Vector2)transform.position + direction * maxRange;
            reticleRenderer.color = Color.red; // Đổi màu cảnh báo ngoài tầm
        }
        else
        {
            // Nếu chuột ở trong tầm, hồng tâm bám sát chuột
            activeReticle.transform.position = mouseWorldPos;
            reticleRenderer.color = Color.green; // Màu hợp lệ
        }
    }

    void ExecuteAction(GameObject prefab, bool isBall)
    {
        if (prefab == null || activeReticle == null) return;

        // 1. Tạo vật thể
        GameObject proj = Instantiate(prefab, transform.position, Quaternion.identity);
        EncounterProjectile script = proj.GetComponent<EncounterProjectile>();

        if (script != null)
        {
            script.isBall = isBall;
            // 2. Gửi vị trí tâm của hồng tâm hiện tại làm điểm đích
            script.SetDestination(activeReticle.transform.position);
        }

        // 3. Xoay vật thể để nhìn cho tự nhiên (hướng về phía tâm)
        Vector2 direction = (activeReticle.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}