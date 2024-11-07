using UnityEngine;

namespace Enemy
{
    public enum EnemyPrefabID
    {
        LE01

    }
    public enum EnemyClass
    {
        Minion,
        Tower,
        Boss,

    }
    [System.Serializable]
    //convert to class if needed
    public struct EnemyDataContainer
    {
        //public GameObject prefab;
        public EnemyPrefabID enemyID;
        public int level;
        public EnemyClass enemyClass;
    }
    [CreateAssetMenu(fileName = "Enemy Variant Data", menuName = "Game Controller/Enemy Variety Data")]
    public class EnemyVariantData : ScriptableObject
    {
        public EnemyDataContainer enemy;

    }
}