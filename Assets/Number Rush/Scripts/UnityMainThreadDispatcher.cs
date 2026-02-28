using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permite encolar acciones para ejecutarlas en el hilo principal de Unity.
/// Necesario porque SMTP corre en un Task (hilo secundario).
/// Adjunta este script a un GameObject en la escena.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("MainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        lock (_queue)
        {
            while (_queue.Count > 0)
                _queue.Dequeue()?.Invoke();
        }
    }

    /// <summary>Encola una acción para ejecutarse en el próximo Update del hilo principal.</summary>
    public static void Enqueue(Action action)
    {
        // Asegura que la instancia exista
        _ = Instance;
        lock (_queue) { _queue.Enqueue(action); }
    }
}