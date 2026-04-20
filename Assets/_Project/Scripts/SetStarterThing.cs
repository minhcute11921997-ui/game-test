using UnityEngine;

public class SetStarterThing : MonoBehaviour
{
    public ThingData starterThing;

    void Awake() // Dùng Awake thay Start để đảm bảo có data trước BattleManager
    {
        if (RuntimeGameState.Party.Count == 0 && starterThing != null)
            RuntimeGameState.Party.Add(starterThing);
    }
}