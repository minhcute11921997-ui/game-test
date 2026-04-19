using UnityEngine;

public class BattlePlayerLoader : MonoBehaviour
{
    void Start()
    {
        if (GlobalPlayerBridge.activeThing == null)
        {
            Debug.LogWarning("Chưa có pet người chơi.");
            return;
        }

        ThingData data = GlobalPlayerBridge.activeThing;

        if (data.battlePrefab != null)
        {
            Instantiate(data.battlePrefab, transform.position, Quaternion.identity);
            Debug.Log("Spawn player pet: " + data.thingName);
        }
        else
        {
            Debug.LogWarning("Thing chưa có battlePrefab.");
        }
    }
}