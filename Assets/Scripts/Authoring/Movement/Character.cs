using UnityEngine;
using Unity.Entities;

public class Character : MonoBehaviour
{
    public float speed = 2f;
    public float rotSpeed = 1f;


}
 
public struct CharacterData : IComponentData 
{
    public float speed;
    public float rotSpeed;
}

public class CharacterBaker : Baker<Character>
{
    public override void Bake(Character authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new CharacterData
        {
            speed = authoring.speed,
            rotSpeed = authoring.rotSpeed
        });

        AddComponent(entity, new InputsData() );
    }
}