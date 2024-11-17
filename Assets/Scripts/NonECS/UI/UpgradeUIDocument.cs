using NonECS.ScriptableObjects;
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

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void OnEnable()
        {
            var root = document.rootVisualElement;
            container = root.Q<VisualElement>("upgrade-container");
        }

        public void ShowUpgradeOptions(Entity entity, int numberRolls)
        {
            targetEntity = entity;
            container.Clear();

            var upgrades = upgradeOptions.GetRandomUpgradeType(numberRolls);
            foreach (var upgrade in upgrades)
            {
                var button = CreateUpgradeButton(upgrade);
                container.Add(button);
            }

            container.style.display = DisplayStyle.Flex;
        }

        private Button CreateUpgradeButton(UpgradeInfo upgrade)
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

            button.clicked += () => ApplyUpgradeAndHideContainer(upgrade);

            return button;
        }

        private void ApplyUpgradeAndHideContainer(UpgradeInfo upgrade)
        {
            if (!entityManager.Exists(targetEntity)) return;

            var aspect = entityManager.GetAspect<UpgradeAspect>(targetEntity);
            aspect.ApplyUpgrade(upgrade);

            HideUpgradeContainer();
        }

        private void HideUpgradeContainer()
        {
            container.style.display = DisplayStyle.None;
        }
    }
}