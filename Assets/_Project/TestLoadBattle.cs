using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLoadBattle : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene("BattleScene");
        }
    }
}