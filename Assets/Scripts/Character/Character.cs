using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterPhysics physics;
    public CharacterPhysics Physics { get => physics; }

    private void Awake()
    {        
        physics = new CharacterPhysics(new CharacterPhysicsData(1, 0.5f), transform);
    }

    private void OnDestroy()
    {
        physics = null;
    }
}
