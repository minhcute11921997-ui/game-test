using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultManager : MonoBehaviour
{
    public bool CheckBattleEnd()
    {
        // Dùng BattleGridManager thay vì FindObjectsByType
        // vì entity đã bị Destroy trước khi hàm này được gọi
        var allEntities = BattleGridManager.Instance.GetAllEntities();

        var team0Alive = new List<BattleEntity>();
        var team1Alive = new List<BattleEntity>();

        foreach (var e in allEntities)
        {
            if (!e.IsDead)
            {
                if (e.TeamId == 0) team0Alive.Add(e);
                else team1Alive.Add(e);
            }
        }

        Debug.Log($"[Result] Team0 alive: {team0Alive.Count} | Team1 alive: {team1Alive.Count}");

        bool team0Wiped = team0Alive.Count == 0;
        bool team1Wiped = team1Alive.Count == 0;

        if (team0Wiped || team1Wiped)
        {
            string winner = team1Wiped ? "PLAYER THẮNG!" : "PLAYER THUA!";
            Debug.Log($"<color=yellow>[BattleResult] {winner}</color>");
            StartCoroutine(ReturnOverworld());
            return true;
        }

        return false;
    }

    IEnumerator ReturnOverworld()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("OverworldScene");
    }
}