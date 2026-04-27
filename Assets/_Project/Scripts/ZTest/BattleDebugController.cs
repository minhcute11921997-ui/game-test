using UnityEngine;

/// Gắn vào bất kỳ GameObject nào trong scene (ví dụ: BattleManager)
/// Tick "Enemy Force Idle" trong Inspector để enemy đứng yên khi test
public class BattleDebugController : MonoBehaviour
{
    public static BattleDebugController Instance { get; private set; }

    [Header("🧪 Debug / Testing")]
    [Tooltip("Tick để tất cả enemy đứng yên, không di chuyển, không tấn công")]
    public bool enemyForceIdle = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
}