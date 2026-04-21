// Assets/_Project/Scripts/Combat/BattleResultManager.cs
using System.Collections;
using System.Collections.Generic;   // ← THÊM DÒNG NÀY
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultManager : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    // Gọi từ BattlePhaseManager sau mỗi lượt
    // Trả về true nếu battle đã kết thúc
    public bool CheckBattleEnd()
    {
        var allEntities = FindObjectsByType<BattleEntity>(FindObjectsSortMode.None);

        var team0 = new List<BattleEntity>();
        var team1 = new List<BattleEntity>();

        foreach (var e in allEntities)
        {
            if (e.TeamId == 0) team0.Add(e);
            else team1.Add(e);
        }

        bool team0Dead = team0.TrueForAll(e => e.CurrentHp <= 0);
        bool team1Dead = team1.TrueForAll(e => e.CurrentHp <= 0);

        if (team1Dead) { StartCoroutine(ShowResult(true)); return true; }
        else if (team0Dead) { StartCoroutine(ShowResult(false)); return true; }

        return false;
    }

    IEnumerator ShowResult(bool win)
    {
        yield return new WaitForSeconds(0.5f);
        if (win) winPanel.SetActive(true);
        else losePanel.SetActive(true);

        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene("OverworldScene"); // đổi tên scene nếu khác
    }
}