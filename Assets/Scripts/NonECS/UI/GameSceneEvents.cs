using System;
using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

namespace NonECS.UI
{
    public class GameSceneEvents : MonoBehaviour
    {
        private UIDocument _document;
        private ProgressBar _healthBar;
        private ProgressBar _expBar;
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

        void Start()
        {
            _document = GetComponent<UIDocument>();
            _healthBar = _document.rootVisualElement.Q("HealthBar") as ProgressBar;
            _expBar = _document.rootVisualElement.Q("ExperienceBar") as ProgressBar;
            if (_healthBar != null) _healthBar.value = 100;
            if (_expBar != null) _expBar.value = 0;

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void LateUpdate()
        {
            if (_targetEntity != Entity.Null)
            {
                FollowEntity();
                return;
            }

            var entityQuery = _entityManager.CreateEntityQuery(typeof(PlayerTag));
            if (entityQuery.HasSingleton<PlayerTag>())
                _targetEntity = entityQuery.GetSingletonEntity();
        }

        void FollowEntity()
        {
            if (!_entityManager.HasComponent<LocalTransform>(_targetEntity)) return;

            var entityPosition = _entityManager.GetComponentData<LocalTransform>(_targetEntity).Position;

            if (Camera.main == null) return;
            var screenPosition = Camera.main.WorldToScreenPoint(entityPosition);

            var uiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);

            if (_healthBar == null) return;

            _healthBar.style.left = uiPosition.x - _healthBar.resolvedStyle.width / 2;
            _healthBar.style.top = uiPosition.y;
        }

        public void UpdateHealth(float newHealth)
        {
            if (_healthBar == null) return;

            _healthBar.value = newHealth;
            //Debug.Log($"New Health Value {newHealth}");
        }
        
        public void UpdateExp(float ExpValue)
        {
            if (_expBar == null) return;

            _expBar.value += ExpValue;
            //Debug.Log($"New Exp Value {_expBar.value}");
        }

    }
}
