using UnityEngine;
using System;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }

    // Eventos del juego
    public event Action OnCustomerMeltdown;
    public event Action<bool> OnPlayerControlChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerCustomerMeltdown()
    {
        OnCustomerMeltdown?.Invoke();
        Debug.Log("GameEvents: Customer meltdown triggered");
    }

    public void SetPlayerControl(bool enabled)
    {
        OnPlayerControlChanged?.Invoke(enabled);
        Debug.Log($"GameEvents: Player control set to {enabled}");
    }
}