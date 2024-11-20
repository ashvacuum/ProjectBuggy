using System;
using Authoring;
using ShipECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

namespace NonECS.UI
{
    public class GameUIView : MonoBehaviour
    {
        private UIDocument _document;
        private ProgressBar _healthBar;
        private ProgressBar _expBar;
        private Label _levelLabel;
        private EntityManager _entityManager;
        private Entity _targetEntity;

        void Start()
        {
            _document = GetComponent<UIDocument>();
            _healthBar = _document.rootVisualElement.Q("HealthBar") as ProgressBar;
            _expBar = _document.rootVisualElement.Q("ExperienceBar") as ProgressBar;
            _levelLabel = _document.rootVisualElement.Q("LevelData") as Label;
            if (_healthBar != null) _healthBar.value = 100;
            if (_expBar != null) _expBar.value = 0;

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void FixedUpdate()
        {
            if ( _entityManager.Exists(_targetEntity))
            {
                FollowEntity();
                UpdateExp();
                UpdateHealth();
                return;
            }

            var entityQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<ExperienceContainer>(), ComponentType.ReadOnly<HealthComponent>(), ComponentType.ReadOnly<LocalTransform>());
            if (!entityQuery.IsEmpty)
                _targetEntity = entityQuery.GetSingletonEntity();
        }

        void FollowEntity()
        {            if (!_entityManager.HasComponent<PlayerTag>(_targetEntity)) return;

            var entityPosition = _entityManager.GetComponentData<LocalTransform>(_targetEntity).Position;

            if (Camera.main == null) return;
            var screenPosition = Camera.main.WorldToScreenPoint(entityPosition);

            var uiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);

            if (_healthBar == null) return;

            _healthBar.style.left = uiPosition.x - _healthBar.resolvedStyle.width / 2;
            _healthBar.style.top = uiPosition.y;
        }

        private void UpdateHealth()
        {

            var newHealth = _entityManager.GetComponentData<HealthComponent>(_targetEntity).HealthPercent;
            if (_healthBar == null) return;

            _healthBar.value = newHealth;
            //Debug.Log($"New Health Value {newHealth}");
        }

        private void UpdateExp()
        {
            var expContainer = _entityManager.GetComponentData<ExperienceContainer>(_targetEntity);
            if (_expBar != null)
            {
                _expBar.value = expContainer.GetExpPercentToNextUpgrade();
                Debug.Log($"Exp Value: {_expBar.value}, {expContainer.TotalExperience}");
            }

            if (_levelLabel != null)
            {
                _levelLabel.text = expContainer.GetCurrentLevel().ToString();
            } 
        }

    }
}
