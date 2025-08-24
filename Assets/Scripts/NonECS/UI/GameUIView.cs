using Authoring;
using ShipECS.Systems;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonECS.UI
{
    public class GameUIView : MonoBehaviour
    {
        [Header("UI Configuration")]
        [SerializeField] private float healthBarYOffset = 50f;
        [SerializeField] private bool enableHealthBarFollow = true;
        
        private UIDocument _document;
        private ProgressBar _healthBar;
        private ProgressBar _expBar;
        private Label _levelLabel;
        private EntityManager _entityManager;
        private Entity _targetEntity;

        public void Initialize(EntityManager entityManager)
        {
            _entityManager = entityManager;
            SetupUIElements();
            
            if (UIManager.Instance != null)
                UIManager.Instance.OnPlayerEntityChanged += OnPlayerEntityChanged;
        }
        
        private void Start()
        {
           // if (_entityManager.)
            //    return;
                
            // Fallback if not initialized by UIManager
            Initialize(World.DefaultGameObjectInjectionWorld.EntityManager);
        }
        
        private void SetupUIElements()
        {
            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                Debug.LogError("UIDocument component not found!");
                return;
            }
            
            _healthBar = _document.rootVisualElement.Q("HealthBar") as ProgressBar;
            _expBar = _document.rootVisualElement.Q("ExperienceBar") as ProgressBar;
            _levelLabel = _document.rootVisualElement.Q("LevelData") as Label;
            
            InitializeDefaultValues();
        }
        
        private void InitializeDefaultValues()
        {
            if (_healthBar != null) _healthBar.value = 100;
            if (_expBar != null) _expBar.value = 0;
            if (_levelLabel != null) _levelLabel.text = "1";
        }
        
        private void OnPlayerEntityChanged(Entity playerEntity)
        {
            _targetEntity = playerEntity;
        }

        private void FixedUpdate()
        {
            if (!_entityManager.Exists(_targetEntity))
                return;
                
            UpdateUIElements();
        }

        private void UpdateUIElements()
        {
            if (enableHealthBarFollow)
                UpdateHealthBarPosition();
            
            UpdateHealth();
            UpdateExperience();
        }
        
        private void UpdateHealthBarPosition()
        {
            if (!_entityManager.HasComponent<PlayerTag>(_targetEntity) || _healthBar == null) 
                return;

            var entityPosition = _entityManager.GetComponentData<LocalTransform>(_targetEntity).Position;

            if (Camera.main == null) return;
            
            var panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                _document.rootVisualElement.panel, 
                entityPosition,
                Camera.main);
           
            _healthBar.style.left = panelPos.x - _healthBar.resolvedStyle.width * 0.5f;
            _healthBar.style.top = panelPos.y + healthBarYOffset;
        }

        private void UpdateHealth()
        {
            if (_healthBar == null || !_entityManager.HasComponent<HealthComponent>(_targetEntity)) 
                return;

            var healthPercent = _entityManager.GetComponentData<HealthComponent>(_targetEntity).HealthPercent;
            _healthBar.value = healthPercent;
        }

        private void UpdateExperience()
        {
            if (!_entityManager.HasComponent<ExperienceContainer>(_targetEntity)) 
                return;
                
            var expContainer = _entityManager.GetComponentData<ExperienceContainer>(_targetEntity);
            
            if (_expBar != null)
            {
                _expBar.value = 1f - expContainer.GetExpPercentToNextUpgrade();
            }

            if (_levelLabel != null)
            {
                _levelLabel.text = expContainer.GetCurrentLevel().ToString();
            } 
        }
        
        private void OnDestroy()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OnPlayerEntityChanged -= OnPlayerEntityChanged;
        }
    }
}
