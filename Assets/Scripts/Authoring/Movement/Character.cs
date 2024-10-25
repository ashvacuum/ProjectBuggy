using UnityEngine;
using Unity.Entities;

public class Character : MonoBehaviour
{
    


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
        

    }
}