using UnityEngine;

public class SleepSystem : MonoBehaviour
{
    [Header("Configuración Sueño")]
    public float maxSleepQuality = 100f;
    public float sleepDeprivationPenalty = 2f;
    public int optimalSleepHour = 22;
    public float baseFatigueRate = 0.5f;

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

        if (timeSinceLastUpdate >= 1f)
        {
            float fatigueRate = CalculateFatigueRate();

            if (gameClock != null && gameClock.IsNightScene)
            {
                fatigueRate *= 1.5f;
            }

            float fatigueThisSecond = fatigueRate * timeSinceLastUpdate;
            currentSleepQuality = Mathf.Clamp(currentSleepQuality - fatigueThisSecond, 0f, maxSleepQuality);
            lastFatigueUpdateTime = currentTime;

            if (Mathf.FloorToInt(currentTime) % 10 == 0)
            {
                SaveSleep();
            }
        }
    }

    float CalculateFatigueRate()
    {
        float sleepPercent = SleepQualityPercent;
        float fatigueRate = baseFatigueRate;

        if (sleepPercent >= 0.8f) fatigueRate *= 0.2f;
        else if (sleepPercent >= 0.6f) fatigueRate *= 0.5f;
        else if (sleepPercent >= 0.4f) fatigueRate *= 0.8f;
        else if (sleepPercent >= 0.2f) fatigueRate *= 1.5f;
        else fatigueRate *= 2f;

        return fatigueRate;
    }

    public void Sleep()
    {
        if (gameClock != null)
        {
            int hourSlept = gameClock.CurrentHour;
            currentSleepQuality = CalculateSleepQuality(hourSlept);
        }
        else
        {
            currentSleepQuality = CalculateSleepQuality(22);
        }

        SaveSleep();
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
            sleepPenalty = hoursAfterOptimal * sleepDeprivationPenalty * 2f;
        }

        return Mathf.Clamp(maxSleepQuality - sleepPenalty, 10f, maxSleepQuality);
    }

    public void ModifySleepQuality(float amount)
    {
        currentSleepQuality = Mathf.Clamp(currentSleepQuality + amount, 0f, maxSleepQuality);
        SaveSleep();
    }

    public string GetFatigueStatus()
    {
        float percent = SleepQualityPercent;
        if (percent >= 0.8f) return "Descansado";
        if (percent >= 0.6f) return "Alerta";
        if (percent >= 0.4f) return "Cansado";
        if (percent >= 0.2f) return "Fatigado";
        return "Agotado";
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
}