using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Prefab placeholder (nếu ThingData chưa có battlePrefab)")]
    public GameObject fallbackPrefab;
    [Header("TEST — kéo ThingData vào đây để test")]
    public ThingData testEnemy;
    public ThingData testPlayer;

    void Start()
    {
        var grid = BattleGridManager.Instance;

        // ── Spawn địch (team 1, bên phải: col 10–17) ───────────
        ThingData enemy = testEnemy != null ? testEnemy : RuntimeGameState.CurrentEnemy;
        ThingData player = testPlayer != null ? testPlayer : GlobalPlayerBridge.activeThing;
        if (enemy != null && enemy.battlePrefab != null)
        {
            var pos = new GridPos(12, 4); // ô spawn mặc định bên phải
            var prefab = enemy.battlePrefab;
            var go = Instantiate(prefab);
            var entity = go.GetComponent<BattleEntity>();
            if (entity == null) entity = go.AddComponent<BattleEntity>();
            entity.Init(enemy, teamId: 1);
            grid.PlaceEntity(entity, pos);
        }
        else Debug.LogWarning("Không có enemy data.");

        // ── Spawn pet người chơi (team 0, bên trái: col 0–7) ───
        
        if (player != null && player.battlePrefab != null)
        {
            var pos = new GridPos(5, 4); // ô spawn mặc định bên trái
            var prefab = player.battlePrefab;
            var go = Instantiate(prefab);
            var entity = go.GetComponent<BattleEntity>();
            if (entity == null) entity = go.AddComponent<BattleEntity>();
            entity.Init(player, teamId: 0);
            grid.PlaceEntity(entity, pos);
        }
        else Debug.LogWarning("Không có player pet data.");
    }
}