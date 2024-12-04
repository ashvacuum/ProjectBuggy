using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace NonECS.UI
{
    // ECS Components
    public struct DamageNumberRequest : IBufferElementData
    {
        public float DamageAmount;
        public float3 WorldPosition;
    }

    // UI Manager MonoBehaviour
    public class DamageNumberUIManager : MonoBehaviour
    {
        
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset damageNumberTemplate;
    [SerializeField] private float floatSpeed = 50f;  // Reduced for top-down view
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float yOffset = 1f;  // Height above the entity
    [SerializeField] private float randomOffset = 0.5f;  // Random horizontal offset

    private EntityManager _entityManager;
    private EntityQuery _damageQuery;
    private Camera _mainCamera;
    private VisualElement _rootElement;
    private List<(VisualElement element, float timeLeft, Vector3 worldPos, Vector2 offset)> _activeNumbers;

    private void OnEnable()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _damageQuery = _entityManager.CreateEntityQuery(typeof(DamageNumberRequest));
        _mainCamera = Camera.main;
        
        _rootElement = document.rootVisualElement;
        _activeNumbers = new List<(VisualElement, float timeLeft, Vector3, Vector2)>();

        // Ensure camera is set up correctly
        if (_mainCamera && !IsValidCameraSetup())
        {
            Debug.LogWarning("Camera might not be properly set up for top-down view. Ensure camera is rotated correctly (typically around X axis).");
        }
    }

    private bool IsValidCameraSetup()
    {
        // Check if camera is roughly pointing downward (between 30 and 90 degrees on X axis)
        return _mainCamera.transform.rotation.eulerAngles.x >= 30f 
            && _mainCamera.transform.rotation.eulerAngles.x <= 90f;
    }

    private bool IsPositionVisible(Vector3 worldPosition)
    {
        Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(worldPosition);
        return viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 
            && viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    private void Update()
    {
        // Process damage buffers
        using (var buffers = _damageQuery.ToEntityArray(Allocator.Temp))
        {
            foreach (var entity in buffers)
            {
                var damageBuffer = _entityManager.GetBuffer<DamageNumberRequest>(entity);
                
                foreach (var damage in damageBuffer)
                {
                    SpawnDamageNumber(damage);
                }
                
                damageBuffer.Clear();
            }
        }

        UpdateDamageNumbers();
    }

    private void SpawnDamageNumber(DamageNumberRequest damage)
    {
        // Add random offset to prevent stacking
        Vector2 randomOffset = new Vector2(
            Random.Range(-this.randomOffset, this.randomOffset),
            Random.Range(-this.randomOffset, this.randomOffset)
        );

        // Apply offset to world position
        Vector3 spawnPosition = new Vector3(
            damage.WorldPosition.x + randomOffset.x,
            damage.WorldPosition.y + yOffset,  // Add height offset
            damage.WorldPosition.z + randomOffset.y
        );

        // Check if position is visible before spawning
        if (!IsPositionVisible(spawnPosition))
            return;

        // Instantiate the damage number
        TemplateContainer damageInstance = damageNumberTemplate.Instantiate();
        VisualElement container = damageInstance.Q<VisualElement>("damage-container");
        Label damageText = damageInstance.Q<Label>("damage-text");
        
        // Set the damage text
        damageText.text = damage.DamageAmount.ToString("F0");  // Removed decimal places
        
        // Convert world position to screen position
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(spawnPosition);
        var CalculatedPosition =
            RuntimePanelUtils.CameraTransformWorldToPanel(document.rootVisualElement.panel, damage.WorldPosition,
                Camera.main);
        CalculatedPosition += randomOffset;
        

        

        // Position the element
        container.style.left = CalculatedPosition.x - container.layout.width / 2;
        container.style.top = CalculatedPosition.y - container.layout.height / 2;

        // Add scale animation
        container.style.scale = new StyleScale(new Scale(Vector3.one * 0.8f));
        container.schedule.Execute(() => {
            container.style.scale = new StyleScale(new Scale(Vector3.one * 1.2f));
        }).StartingIn(0);
        container.schedule.Execute(() => {
            container.style.scale = new StyleScale(new Scale(Vector3.one));
        }).StartingIn(100);

        // Add to root and active numbers list
        _rootElement.Add(container);
        _activeNumbers.Add((container, lifetime, spawnPosition, randomOffset));
    }

    private void UpdateDamageNumbers()
    {
        for (int i = _activeNumbers.Count - 1; i >= 0; i--)
        {
            var (element, timeLeft, worldPos, offset) = _activeNumbers[i];
            
            // Update position
            worldPos.y += floatSpeed * Time.deltaTime;  // Float upward in world space
            
            // Convert to screen space
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
            
            // Remove if behind camera
            if (screenPos.z < 0)
            {
                _rootElement.Remove(element);
                _activeNumbers.RemoveAt(i);
                continue;
            }

            // Update position
            Vector2 elementPos = RuntimePanelUtils.ScreenToPanel(
                _rootElement.panel,
                new Vector2(screenPos.x, Screen.height - screenPos.y)
            );

            element.style.left = elementPos.x - element.layout.width / 2;
            element.style.top = elementPos.y - element.layout.height / 2;

            // Update time and opacity
            float newTime = timeLeft - Time.deltaTime;
            if (newTime <= 0)
            {
                _rootElement.Remove(element);
                _activeNumbers.RemoveAt(i);
            }
            else
            {
                // Fade out only in the last 30% of lifetime
                if (newTime < lifetime * 0.3f)
                {
                    element.style.opacity = newTime / (lifetime * 0.3f);
                }
                _activeNumbers[i] = (element, newTime, worldPos, offset);
            }
        }
    }
    }
}