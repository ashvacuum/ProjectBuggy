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
        if (_timeEntity != null) {
            DisplayTime();
        }
    }

    private void DisplayTime()
    {
        try
        {
            var curTimeEntity = _entityManager.GetComponentData<TimeManagerComponent>(_timeEntity).CurrentTime;
            int minutes = Mathf.FloorToInt(curTimeEntity / 60); // Calculate minutes
            int seconds = Mathf.FloorToInt(curTimeEntity % 60); // Calculate remaining seconds

            string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);
            timerText.text = formattedTime;  
        }
        catch
        {
            return;
        }

    }
}
