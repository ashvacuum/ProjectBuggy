using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Entities;

public class ContollerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private Entity _timeEntity;
    private EntityManager _entityManager;

    private IEnumerator Start()
    {
        //hacky solution
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        yield return new WaitForSeconds(0.2f);
        _timeEntity = _entityManager.CreateEntityQuery(typeof(TimeManagerComponent)).GetSingletonEntity();  
        Debug.Log("  Time: "+_timeEntity.ToString());

    }

    private void Update()
    {
        if (_timeEntity != Entity.Null) {
            DisplayTime();
        }
    }

    private void DisplayTime()
    {
        timerText.text = GetTime();
    }

    public string GetTime()
    {
        try
        {
            var curTimeEntity = _entityManager.GetComponentData<TimeManagerComponent>(_timeEntity).CurrentTime;
            var minutes = Mathf.FloorToInt(curTimeEntity / 60); // Calculate minutes
            var seconds = Mathf.FloorToInt(curTimeEntity % 60); // Calculate remaining seconds

            return $"{minutes:00}:{seconds:00}";
            
        }
        catch
        {
            return string.Empty;
        }
    }
}
