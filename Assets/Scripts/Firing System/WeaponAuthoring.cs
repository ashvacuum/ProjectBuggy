using Unity.Entities;
using Unity.Collections;

public class WeaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public WeaponData[] weaponDataArray; // Assign this in the inspector

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var weaponComponent = new WeaponComponent
        {
            EquippedWeapons = new NativeList<WeaponData>(Allocator.Persistent), // Initialize the list
            ActiveWeaponIndex = 0 // Set default active weapon index
        };

        // Add weapon data to the NativeList
        foreach (var weaponData in weaponDataArray)
        {
            weaponComponent.EquippedWeapons.Add(weaponData);
        }

        dstManager.AddComponentData(entity, weaponComponent);
    }
}