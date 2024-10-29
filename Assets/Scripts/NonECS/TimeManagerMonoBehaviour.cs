using UnityEngine;
using Unity.Entities;

public class TimeManagerMonoBehaviour : MonoBehaviour
{
    /*private EntityManager entityManager;
    private Entity timeEntity;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // Find the entity with TimeManagerComponent
        foreach (var entity in entityManager.CreateEntityQuery(typeof(TimeManagerComponent)).ToEntityArray(Unity.Collections.Allocator.Temp))
        {
            timeEntity = entity;
            break;
        }
    }

    void OnGUI()
    {
        if (entityManager == null || !entityManager.Exists(timeEntity)) return;

        var timeManager = entityManager.GetComponentData<TimeManagerComponent>(timeEntity);
        var timeData = entityManager.GetComponentData<TimeDataComponent>(timeEntity);
        float remainingTime = timeData.TimeBlob.Value.MaxTime - timeManager.CurrentTime;

        GUILayout.Label($"Time Remaining: {remainingTime:F1}");
        
        if (GUILayout.Button(timeManager.IsPaused ? "Resume" : "Pause"))
        {
            timeManager.IsPaused = !timeManager.IsPaused;
            entityManager.SetComponentData(timeEntity, timeManager);
        }

        if (GUILayout.Button("Reset"))
        {
            timeManager.CurrentTime = 0f;
            timeManager.IsRunning = true;
            timeManager.IsPaused = false;
            entityManager.SetComponentData(timeEntity, timeManager);
        }
    }*/
}