using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEnemy : MonoBehaviour
{
    private ThingData data;
    private int currentHP;

    void Start()
    {
        data = GlobalBattleBridge.encounteredThing;

        if (data != null)
        {
            currentHP = data.hp;
            Debug.Log(data.thingName + " HP: " + currentHP);
        }
        else
        {
            Debug.LogWarning("Không có ThingData.");
            currentHP = 50;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        Debug.Log(data.thingName + " HP còn: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(data.thingName + " bị hạ!");

        Destroy(gameObject);

        SceneManager.LoadScene("MainScene");
    }
}