using UnityEngine;

[CreateAssetMenu(menuName = "Game Controller/Time Data")]
public class TimeData : ScriptableObject
{
    public int maxTimeInMinutes = 15; 
    public float AfterBossTime = 5f; 
}