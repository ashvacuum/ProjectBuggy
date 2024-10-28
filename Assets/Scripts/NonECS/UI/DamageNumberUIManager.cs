using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonECS.UI
{
    // ECS Components
    public struct DamageNumberRequest : IComponentData
    {
        public float DamageAmount;
        public float3 WorldPosition;
    }

    public struct ActiveDamageNumber : IComponentData
    {
        public float DamageAmount;
        public float TimeAlive;
        public float3 WorldPosition;
        public float2 RandomOffset;
        public int UIElementId; // Track which UI element this corresponds to
    }

// Singleton component to track the next available ID
    public struct DamageNumberUICounter : IComponentData
    {
        public int NextId;
    }

// UI Manager MonoBehaviour
    public class DamageNumberUIManager : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset damageNumberTemplate;
    
        private VisualElement _rootElement;
        private readonly Dictionary<int, VisualElement> _activeDamageNumbers = new();

        private void Start()
        {
            _rootElement = uiDocument.rootVisualElement;
            // Create singleton entity for ID tracking
            var world = World.DefaultGameObjectInjectionWorld;
            var entity = world.EntityManager.CreateEntity();
            world.EntityManager.AddComponentData(entity, new DamageNumberUICounter { NextId = 0 });
        }

        public int CreateDamageNumber(float amount, Vector3 worldPosition)
        {
            // Instantiate the damage number template
            var element = damageNumberTemplate.Instantiate();
            element.style.position = Position.Absolute;
        
            // Set the damage text
            var textElement = element.Q<Label>("damage-text");
            textElement.text = amount.ToString("F0");
        
            // Add to root and dictionary
            _rootElement.Add(element);

            var idQuery =
                World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(new ComponentType[]
                    { typeof(DamageNumberUICounter) });

            var id = 0;
            idQuery.TryGetSingletonEntity<DamageNumberUICounter>(out Entity EntityFromQuery);
            if (EntityFromQuery != Entity.Null)
            {
                id = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<DamageNumberUICounter>(EntityFromQuery).NextId;
                id++;
                _activeDamageNumbers[id] = element;
                return id;
            }
            
            _activeDamageNumbers[id] = element;
            return id;
        }

        public void UpdateDamageNumber(int id, Vector3 position, float alpha)
        {
            if (!_activeDamageNumbers.TryGetValue(id, out var element))
                return;

            if (Camera.main != null)
            {
                var screenPos = Camera.main.WorldToScreenPoint(position);
                // Don't show if behind camera
                if (screenPos.y < 0)
                {
                    element.style.display = DisplayStyle.None;
                    return;
                }

                element.style.display = DisplayStyle.Flex;
                element.style.left = screenPos.x;
                element.style.top = Screen.height - screenPos.y;
            }

            element.style.opacity = alpha;
        }

        public void RemoveDamageNumber(int id)
        {
            if (!_activeDamageNumbers.TryGetValue(id, out var element))
                return;

            _rootElement.Remove(element);
            _activeDamageNumbers.Remove(id);
        }
    }
}