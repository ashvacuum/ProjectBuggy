using Unity.Entities;
using UnityEngine;
using Enemy;

[CreateAssetMenu(fileName = "EnemyVariant", menuName = "Game Controller/EnemyPrefabMapping")]
public class EnemyPrefabMapping : ScriptableObject
{
    //public EnemyPrefabEntry[] entries;
    public EnemyPrefabID enemyID;
    public GameObject prefab;
}

[System.Serializable]
public struct EnemyPrefabEntry
{
    public EnemyPrefabID enemyID;
    public GameObject prefab;
}
