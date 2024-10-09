using System;
using Authoring;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class GameSceneEvents : MonoBehaviour
{
    private UIDocument _document;
    private ProgressBar _progressBar;
    
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
    }
    
    public void UpdateHealth(float newHealth)
    {
        if (_progressBar == null) return;
        
        
        _progressBar.value = newHealth;
        Debug.Log($"New Value {newHealth}");
    }

}
