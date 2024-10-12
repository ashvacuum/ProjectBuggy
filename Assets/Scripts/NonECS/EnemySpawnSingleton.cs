using System;
using UnityEngine;

namespace NonECS
{
    public class EnemySpawnSingleton : MonoBehaviour
    {
        public int InitialNumberEnemies = 5;
        public AnimationCurve EnemySpawnCurve;
        public int MaximumEnemies = 3000;
        public float MaximumTime = 1800f;

        
        public static EnemySpawnSingleton Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public int GetNumEnemiesBasedOnTime(float time)
        {
            time = Mathf.InverseLerp(0, MaximumTime, time);
            var lerpTime = EnemySpawnCurve.Evaluate(time);
            return Mathf.FloorToInt(Mathf.Lerp(InitialNumberEnemies, MaximumEnemies, lerpTime));
        }
    }
}
