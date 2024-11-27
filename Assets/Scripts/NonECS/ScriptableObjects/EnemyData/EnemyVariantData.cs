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

    [CreateAssetMenu(fileName = "Enemy Variant Data", menuName = "Game Controller/Enemy Variety Data")]
    public class EnemyVariantData : ScriptableObject
    {
        public GameObject prefab;
        public EnemyPrefabID enemyID;
        public int level;
        public EnemyClass enemyClass;

    }
}