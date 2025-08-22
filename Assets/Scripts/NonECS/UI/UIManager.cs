using System;
using Unity.Entities;
using UnityEngine;

namespace NonECS.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameUIView gameUIView;
        [SerializeField] private UpgradeUIController upgradeUIController;
        
        private EntityManager _entityManager;
        private Entity _playerEntity;
        
        public static UIManager Instance { get; private set; }
        
        public event Action<Entity> OnPlayerEntityChanged;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            if (gameUIView != null)
                gameUIView.Initialize(_entityManager);
                
            if (upgradeUIController != null)
                upgradeUIController.Initialize(_entityManager);
        }
        
        private void Update()
        {
            UpdatePlayerEntity();
        }
        
        private void UpdatePlayerEntity()
        {
            var entityQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerTag>(),
                ComponentType.ReadOnly<ExperienceContainer>(),
                ComponentType.ReadOnly<HealthComponent>(),
                ComponentType.ReadOnly<LocalTransform>()
            );
            
            if (!entityQuery.IsEmpty)
            {
                var newPlayerEntity = entityQuery.GetSingletonEntity();
                if (newPlayerEntity != _playerEntity)
                {
                    _playerEntity = newPlayerEntity;
                    OnPlayerEntityChanged?.Invoke(_playerEntity);
                }
            }
            
            entityQuery.Dispose();
        }
        
        public Entity GetPlayerEntity() => _playerEntity;
        public EntityManager GetEntityManager() => _entityManager;
    }
}