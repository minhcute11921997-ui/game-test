using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultManager : MonoBehaviour
{
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

        if (team0Dead || team1Dead)
        {
            StartCoroutine(ReturnOverworld());
            return true;
        }
        return false;
    }

    IEnumerator ReturnOverworld()
    {
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene("OverworldScene");
    }
}