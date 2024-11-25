using System;
using Authoring;
using NonECS.ScriptableObjects;
using ShipECS.Entities;
using ShipECS.Systems;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonECS.UI
{
    public class UpgradeUIDocument : MonoBehaviour
    {
        private UIDocument document;
        private VisualElement container;
        private EntityManager entityManager;
        private Entity targetEntity;
        [SerializeField] private UpgradeOptions upgradeOptions;
        
        private EntityQuery levelUpQuery;
        private EntityQuery aspectQuery;
        private EntityQuery timeManagerQuery;
        private int _currentLevelUpBuffers;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Start()
        {
            _currentLevelUpBuffers = 0;
            levelUpQuery = entityManager.CreateEntityQuery(typeof(LevelUpBuffer));
            aspectQuery = entityManager.CreateEntityQuery(typeof(PlayerTag));
            timeManagerQuery = entityManager.CreateEntityQuery(typeof(TimeManagerComponent));
        }

        private void OnEnable()
        {
            var root = document.rootVisualElement;
            container = root.Q<VisualElement>("upgrade-container");
        }

        private void FixedUpdate()
        {
            if (!levelUpQuery.TryGetSingletonBuffer<LevelUpBuffer>(out var levelUpBuffer)) return;
            if (levelUpBuffer.IsEmpty)
            {
                Debug.Log("Level up buffers are empty");
                return;
            }
            
            var aspectEntity = aspectQuery.GetSingletonEntity();
            if (aspectEntity == Entity.Null) return;
            
            ShowUpgradeOptions(aspectEntity, 3);
        }

        private void ShowUpgradeOptions(Entity entity, int numberRolls)
        {
            targetEntity = entity;
            container.Clear();

            var upgrades = upgradeOptions.GetRandomUpgradeType(numberRolls);
            var aspect = entityManager.GetAspect<UpgradeAspects>(targetEntity);
            foreach (var upgrade in upgrades)
            {
                var button = CreateUpgradeButton(upgrade, aspect);
                container.Add(button);
            }

            container.style.display = DisplayStyle.Flex;
        }

        private Button CreateUpgradeButton(UpgradeInfo upgrade, UpgradeAspects aspect)
        {
            var button = new Button();
            button.AddToClassList("upgrade-button");

            var layout = new VisualElement();
            layout.AddToClassList("upgrade-layout");

            var title = new Label(upgrade.UpgradeType.ToString());
            title.AddToClassList("upgrade-title");

            var level = new Label($"Level: {upgrade.UpgradeLevels[0]}");
            level.AddToClassList("upgrade-level");

            layout.Add(title);
            layout.Add(level);
            button.Add(layout);

            button.clicked += () => ApplyUpgradeAndHideContainer(upgrade, aspect);

            return button;
        }

        private void ApplyUpgradeAndHideContainer(UpgradeInfo upgrade, UpgradeAspects aspect)
        {
            if (!entityManager.Exists(targetEntity)) return;

            
            var upgradeLevel = Mathf.Min(aspect.GetUpgradeLevel(upgrade.UpgradeType), upgrade.UpgradeLevels.Count - 1);
            aspect.ApplyUpgrades(upgrade.UpgradeType, upgrade.UpgradeLevels[upgradeLevel]);


            HideUpgradeContainer();
        }

        private void HideUpgradeContainer()
        {
            container.style.display = DisplayStyle.None;
        }
    }
}