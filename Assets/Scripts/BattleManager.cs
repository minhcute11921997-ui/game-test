using UnityEngine; // 1. Bắt buộc phải có dòng này để dùng Vector3, Instantiate...

// 2. Tên Class phải trùng khớp hoàn toàn với tên File (BattleManager)
public class BattleManager : MonoBehaviour
{
    void Start()
    {
        // 3. Lấy dữ liệu con Thing từ Overworld gửi sang qua "Cầu"
        ThingData enemyToSpawn = GlobalBattleBridge.encounteredThing;

        if (enemyToSpawn != null)
        {
            // Kiểm tra xem bạn đã gán Battle Prefab trong ThingData chưa
            if (enemyToSpawn.battlePrefab != null)
            {
                // Tạo con Thing tại vị trí ô (5, 0, 5) trên lưới 8x8
                Instantiate(enemyToSpawn.battlePrefab, new Vector3(5, 0, 5), Quaternion.identity);

                Debug.Log($"Trận đấu bắt đầu! Đối thủ: {enemyToSpawn.thingName}");
            }
            else
            {
                Debug.LogError("Lỗi: Bạn chưa gán Battle Prefab cho Thing này!");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy dữ liệu Thing từ Overworld. Có thể bạn đã mở thẳng Scene Battle?");
        }
    }
}