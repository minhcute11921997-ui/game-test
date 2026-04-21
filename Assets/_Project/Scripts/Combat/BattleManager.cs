using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject fallbackPrefab;

    // Vị trí mặc định trên lưới cho tối đa 2 unit mỗi phe
    static readonly (int col, int row)[] PlayerPositions = { (3, 2), (3, 5) };
    static readonly (int col, int row)[] EnemyPositions  = { (14, 2), (14, 5) };

    void Start()
    {
        var playerParty = RuntimeGameState.Party;
        var enemyParty  = RuntimeGameState.EnemyParty;

        // Backward compat: nếu EnemyParty trống, dùng CurrentEnemy
        if (enemyParty.Count == 0 && RuntimeGameState.CurrentEnemy != null)
            enemyParty = new List<ThingData> { RuntimeGameState.CurrentEnemy };

        if (playerParty.Count == 0) { Debug.LogWarning("[Battle] Party rỗng!"); return; }
        if (enemyParty.Count  == 0) { Debug.LogWarning("[Battle] Không có enemy!"); return; }

        var grid = BattleGridManager.Instance;

        // Spawn player party (tối đa 2)
        for (int i = 0; i < Mathf.Min(playerParty.Count, PlayerPositions.Length); i++)
            SpawnEntity(grid, playerParty[i], teamId: 0, PlayerPositions[i].col, PlayerPositions[i].row);

        // Spawn enemy party (tối đa 2)
        for (int i = 0; i < Mathf.Min(enemyParty.Count, EnemyPositions.Length); i++)
            SpawnEntity(grid, enemyParty[i], teamId: 1, EnemyPositions[i].col, EnemyPositions[i].row);

        // Sprint 9: đánh giá synergy sau khi spawn xong
        if (SynergyManager.Instance != null)
            SynergyManager.Instance.EvaluateSynergies();
    }

    void SpawnEntity(BattleGridManager grid, ThingData data, int teamId, int col, int row)
    {
        GameObject prefab = data.battlePrefab != null ? data.battlePrefab : fallbackPrefab;
        if (prefab == null) { Debug.LogError($"[Spawn] Không có prefab cho {data.thingName}!"); return; }

        var go     = Instantiate(prefab);
        var entity = go.GetComponent<BattleEntity>() ?? go.AddComponent<BattleEntity>();
        entity.Init(data, teamId);
        grid.PlaceEntity(entity, new GridPos(col, row));

        // Spawn HP bar
        var hpBarPrefab = Resources.Load<GameObject>("HpBar");
        if (hpBarPrefab != null)
        {
            var hpBarGo = Instantiate(hpBarPrefab);
            var hpBar   = hpBarGo.GetComponent<EntityHpBar>();
            hpBar.Init(entity.transform);
            hpBar.SetHp(data.hp, data.hp);
            entity.hpBar = hpBar;
        }
        else
        {
            Debug.LogWarning("[HpBar] Prefab chưa có trong Resources/ — bỏ qua");
        }
    }
}