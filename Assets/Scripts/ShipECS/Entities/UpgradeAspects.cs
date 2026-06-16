using NonECS.ScriptableObjects;
using Unity.Entities;

namespace ShipECS.Entities
{
    public struct ShipUpgradeLevels : IBufferElementData
    {
        public UpgradeType type;
        public int level;
    }
}
