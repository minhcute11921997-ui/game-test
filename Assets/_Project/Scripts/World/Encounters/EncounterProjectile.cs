using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterProjectile : MonoBehaviour
{
    [Header("Cấu hình bay")]
    public float speed = 15f;
    public bool isBall = false;
    public enum EncounterProjectileType
    {
        CaptureBall,
        BattleShot
    }
    public EncounterProjectileType projectileType;

    [Header("Cơ chế Thu phục")]
    public float captureChance = 40f;

    private Vector3 targetDestination;
    private bool hasReachedDestination = false;

    // Được gọi từ PlayerActions khi bắn
    public void SetDestination(Vector3 destination)
    {
        targetDestination = destination;
    }

    void Update()
    {
        if (hasReachedDestination) return;

        // Di chuyển đến điểm đích (tâm hồng tâm)
        transform.position = Vector3.MoveTowards(transform.position, targetDestination, speed * Time.deltaTime);

        // Kiểm tra xem đã chạm đích chưa
        if (Vector2.Distance(transform.position, targetDestination) < 0.1f)
        {
            OnDestinationReached();
        }
    }

    void OnDestinationReached()
    {
        if (hasReachedDestination) return;
        hasReachedDestination = true;

        // Quét một vòng tròn nhỏ tại điểm đích để tìm Thing
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.4f, LayerMask.GetMask("Monster"));

        if (hit != null)
        {
            ShadowRoaming thing = hit.GetComponent<ShadowRoaming>();
            if (thing != null)
            {
                if (isBall) TryCaptureEncounter(thing);
                else StartEncounterBattle(thing);
            }
        }
        else
        {
            Debug.Log("Hụt mục tiêu.");
        }

        Destroy(gameObject); // Xóa viên đạn sau khi nổ/chạm đích
    }

    void TryCaptureEncounter(ShadowRoaming thing)
    {
        float roll = Random.Range(0f, 100f);
        if (roll <= captureChance)
        {
            Debug.Log("<color=green>THÀNH CÔNG! Đã bắt được Thing.</color>");
            thing.OnCaptured(); // Gọi hàm biến mất và báo về Manager
        }
        else
        {
            Debug.Log("<color=yellow>Hụt rồi! Thing đã chạy thoát.</color>");
        }
    }

    void StartEncounterBattle(ShadowRoaming thing)
    {
        Debug.Log("<color=red>KHIÊU CHIẾN! Chuyển sang cảnh 8x8.</color>");

        // 1. Nạp dữ liệu vào Cầu nối
        RuntimeGameState.CurrentEnemy = thing.myData;

        // 2. Báo cho Thing biến mất ở Overworld (để giải phóng slot cho bụi cỏ)
        thing.OnCaptured();

        // 3. Chuyển Scene
        SceneManager.LoadScene("BattleScene");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}