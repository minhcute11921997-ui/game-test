using UnityEngine;

public class SetStarterThing : MonoBehaviour
{
    public ThingData starterThing;

    void Start()
    {
        GlobalPlayerBridge.activeThing = starterThing;
    }
}