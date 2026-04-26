using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultManager : MonoBehaviour
{
    public bool CheckBattleEnd()
    {
        // GetAllEntities() chỉ trả về entity còn trong _occupied dict
        // (entity chết đã bị RemoveEntity() + Destroy() trước đó)
        var allAlive = BattleGridManager.Instance.GetAllEntities();

        bool hasTeam0 = false;
        bool hasTeam1 = false;

        foreach (var e in allAlive)
        {
            if (e.TeamId == 0) hasTeam0 = true;
            else hasTeam1 = true;
        }

        Debug.Log($"[Result] Team0 còn sống: {hasTeam0} | Team1 còn sống: {hasTeam1}");

        // Chỉ check khi 2 phe đã từng xuất hiện (tránh false positive lúc mới spawn)
        // Nếu một phe bị xóa hết khỏi grid → trận kết thúc
        if (!hasTeam0 || !hasTeam1)
        {
            // Tránh trigger khi cả 2 đều chưa spawn
            if (!hasTeam0 && !hasTeam1) return false;

            string result = !hasTeam1 ? "PLAYER THẮNG!" : "PLAYER THUA!";
            Debug.Log($"<color=yellow>[BattleResult] === {result} ===</color>");
            StartCoroutine(ReturnOverworld());
            return true;
        }

        return false;
    }

    IEnumerator ReturnOverworld()
    {
        yield return new WaitForSeconds(1.5f);
        Debug.Log("[BattleResult] Load OverworldScene...");
        SceneManager.LoadScene("OverworldScene");
    }
}