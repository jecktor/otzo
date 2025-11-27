using UnityEngine;

public class SleepSystem : MonoBehaviour
{
    [Header("Configuración Sueño")]
    public float maxSleepQuality = 100f;
    public float sleepDeprivationPenalty = .1f;
    public int optimalSleepHour = 22;
    public float baseFatigueRate = 0.01f; // REDUCIDO de 0.5f a 0.2f
    public float maxSleepRecovery = 100f; // NUEVO: Máximo recuperable al dormir

    private float currentSleepQuality;
    private float fatigueAccumulator = 0f;
    private float lastFatigueUpdateTime = 0f;
    private GameClock gameClock;

    public float CurrentSleepQuality => currentSleepQuality;
    public float SleepQualityPercent => currentSleepQuality / maxSleepQuality;

    // Propiedad para verificar si el sistema de sueño está pausado
    public bool IsPaused => DayNightTransitionManager.Instance != null &&
                           DayNightTransitionManager.Instance.IsTransitioning;

    void Start()
    {
        currentSleepQuality = PlayerPrefs.GetFloat("SleepQuality", maxSleepQuality);
        lastFatigueUpdateTime = Time.time;

        if (GameManagerPersistent.Instance != null)
        {
            gameClock = GameManagerPersistent.Instance.gameClock;
        }
        if (gameClock == null)
        {
            gameClock = FindFirstObjectByType<GameClock>();
        }

        Debug.Log($"😴 Sistema de sueño iniciado - Calidad actual: {currentSleepQuality:F1}%");
    }

    void Update()
    {
        if (!enabled) return;

        // PAUSAR durante transiciones
        if (IsPaused)
        {
            return;
        }

        UpdateFatigue();
    }

    void UpdateFatigue()
    {
        float currentTime = Time.time;
        float timeSinceLastUpdate = currentTime - lastFatigueUpdateTime;

        // **CORRECCIÓN: Actualizar cada 2 segundos en lugar de cada segundo**
        if (timeSinceLastUpdate >= 2f) // CAMBIADO de 1f a 2f
        {
            float fatigueRate = CalculateFatigueRate();

            if (gameClock != null && gameClock.IsNightScene)
            {
                fatigueRate *= 1.3f; // REDUCIDO de 1.5f a 1.3f
            }

            float fatigueThisUpdate = fatigueRate * (timeSinceLastUpdate / 2f); // Ajustado por el nuevo intervalo
            currentSleepQuality = Mathf.Clamp(currentSleepQuality - fatigueThisUpdate, 0f, maxSleepQuality);
            lastFatigueUpdateTime = currentTime;

            // **CORRECCIÓN: Guardar menos frecuentemente**
            if (Mathf.FloorToInt(currentTime) % 30 == 0) // CAMBIADO de 10 a 30 segundos
            {
                SaveSleep();
            }
        }
    }

    float CalculateFatigueRate()
    {
        float sleepPercent = SleepQualityPercent;
        float fatigueRate = baseFatigueRate;

        // **CORRECCIÓN: Tasas de fatiga más balanceadas**
        if (sleepPercent >= 0.8f) fatigueRate *= 0.1f;      // Muy descansado: fatiga muy lenta
        else if (sleepPercent >= 0.6f) fatigueRate *= 0.3f; // Descansado: fatiga lenta
        else if (sleepPercent >= 0.4f) fatigueRate *= 0.6f; // Normal: fatiga moderada
        else if (sleepPercent >= 0.2f) fatigueRate *= 1.2f; // Cansado: fatiga rápida
        else fatigueRate *= 2f;                             // Agotado: fatiga muy rápida

        return fatigueRate;
    }

    public void Sleep()
    {
        float sleepQualityBefore = currentSleepQuality;

        if (gameClock != null)
        {
            int hourSlept = gameClock.CurrentHour;
            currentSleepQuality = CalculateSleepQuality(hourSlept);
        }
        else
        {
            currentSleepQuality = CalculateSleepQuality(22);
        }

        // **NUEVA LÓGICA: Si tenía más de 70% antes de dormir, establecer a 70% máximo**
        if (sleepQualityBefore >= 95f)
        {
            currentSleepQuality = 95;
            Debug.Log($"🛌 Sueño limitado a {maxSleepRecovery}% (tenías {sleepQualityBefore:F1}%)");
        }

        SaveSleep();

        Debug.Log($"😴 Durmiendo - Antes: {sleepQualityBefore:F1}%, Después: {currentSleepQuality:F1}%");
    }

    public float CalculateSleepQuality(int hourSlept)
    {
        float sleepPenalty = 0f;

        if (hourSlept < optimalSleepHour)
        {
            int hoursBeforeOptimal = optimalSleepHour - hourSlept;
            sleepPenalty = hoursBeforeOptimal * sleepDeprivationPenalty;
        }
        else
        {
            int hoursAfterOptimal = hourSlept - optimalSleepHour;
            sleepPenalty = hoursAfterOptimal * sleepDeprivationPenalty * 1.5f; // REDUCIDO de 2f a 1.5f
        }

        // **CORRECCIÓN: Mínimo de sueño aumentado de 10f a 20f**
        return Mathf.Clamp(maxSleepQuality - sleepPenalty, 20f, maxSleepQuality);
    }

    public void ModifySleepQuality(float amount)
    {
        float oldQuality = currentSleepQuality;
        currentSleepQuality = Mathf.Clamp(currentSleepQuality + amount, 0f, maxSleepQuality);
        SaveSleep();

        Debug.Log($"📊 Sueño modificado: {oldQuality:F1}% → {currentSleepQuality:F1}% ({amount:+#;-#;0})");
    }

    public string GetFatigueStatus()
    {
        float percent = SleepQualityPercent;
        if (percent >= 0.8f) return "💪 Descansado";
        if (percent >= 0.6f) return "😊 Alerta";
        if (percent >= 0.4f) return "😐 Cansado";
        if (percent >= 0.2f) return "😫 Fatigado";
        return "😴 Agotado";
    }

    void SaveSleep()
    {
        PlayerPrefs.SetFloat("SleepQuality", currentSleepQuality);
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Sleep")]
    public void ResetSleep()
    {
        currentSleepQuality = maxSleepQuality;
        SaveSleep();
        Debug.Log("😴 Sueño reseteado al 100%");
    }

    [ContextMenu("Set Sleep to 50%")]
    public void SetSleepTo50Percent()
    {
        currentSleepQuality = maxSleepQuality * 0.5f;
        SaveSleep();
        Debug.Log("😴 Sueño establecido al 50%");
    }

    [ContextMenu("Set Sleep to 80%")]
    public void SetSleepTo80Percent()
    {
        currentSleepQuality = maxSleepQuality * 0.8f;
        SaveSleep();
        Debug.Log("😴 Sueño establecido al 80%");
    }

    // **NUEVO: Método para debug del sistema de sueño**
    public void DebugSleepInfo()
    {
        Debug.Log($"=== DEBUG SUEÑO ===");
        Debug.Log($"Calidad actual: {currentSleepQuality:F1}%");
        Debug.Log($"Porcentaje: {SleepQualityPercent:P1}");
        Debug.Log($"Estado: {GetFatigueStatus()}");
        Debug.Log($"Tasa de fatiga base: {baseFatigueRate:F3}");
        Debug.Log($"Tasa actual: {CalculateFatigueRate():F3}");
        Debug.Log($"Máximo recuperable: {maxSleepRecovery}%");
    }
}