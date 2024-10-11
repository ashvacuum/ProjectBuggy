using System;
using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class GameSceneEvents : MonoBehaviour
{
    private UIDocument _document;
    private ProgressBar _progressBar;
    private EntityManager _entityManager;
    private Entity _targetEntity;
    
    public static GameSceneEvents Instance { get; private set; }

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _document = GetComponent<UIDocument>();
        _progressBar = _document.rootVisualElement.Q("ProgressBar") as ProgressBar;
        if (_progressBar != null) _progressBar.value = 100;

        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        

       

    }

    private void LateUpdate()
    {
        if (_targetEntity != Entity.Null)
        {
            FollowEntity();
            return;
        }
        
        var entityQuery = _entityManager.CreateEntityQuery(typeof(ShipComponent));
        if(entityQuery.HasSingleton<ShipComponent>())
            _targetEntity = entityQuery.GetSingletonEntity();
    }

    void FollowEntity()
    {
        if (!_entityManager.HasComponent<LocalTransform>(_targetEntity)) return;

        var entityPosition = _entityManager.GetComponentData<LocalTransform>(_targetEntity).Position;

        if (Camera.main == null) return;
        var screenPosition = Camera.main.WorldToScreenPoint(entityPosition);

        var uiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        
        if (_progressBar == null) return;
        
        _progressBar.style.left = uiPosition.x - _progressBar.resolvedStyle.width / 2;
        _progressBar.style.top = uiPosition.y;
    }

    public void UpdateHealth(float newHealth)
    {
        if (_progressBar == null) return;
        
        _progressBar.value = newHealth;
        Debug.Log($"New Value {newHealth}");
    }

}
