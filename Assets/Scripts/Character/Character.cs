using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterData data;
    public CharacterData Data { get => data; }

    private CharacterPhysics physics;
    public CharacterPhysics Physics { get => physics; }

    public void Init(CharacterData data)
    {
        this.data = data;
        physics = new CharacterPhysics(new CharacterPhysicsData(1, 0.5f), transform);
    }

    private void OnDestroy()
    {
        physics = null;
    }
}
