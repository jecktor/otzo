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

    // Styles para UI
    private GUIStyle sleepStyle;

    public float CurrentSleepQuality => currentSleepQuality;
    public float SleepQualityPercent => currentSleepQuality / maxSleepQuality;

    void Start()
    {
        currentSleepQuality = maxSleepQuality;
        lastFatigueUpdateTime = Time.time;
        CreateGUIStyle();

        // Obtener referencia al GameClock
        if (GameManagerPersistent.Instance != null)
        {
            gameClock = GameManagerPersistent.Instance.gameClock;
        }
        if (gameClock == null)
        {
            gameClock = FindFirstObjectByType<GameClock>();
        }
    }

    void CreateGUIStyle()
    {
        sleepStyle = new GUIStyle();
        sleepStyle.fontSize = 16;
        sleepStyle.normal.textColor = Color.white;
        sleepStyle.alignment = TextAnchor.UpperLeft;
        sleepStyle.fontStyle = FontStyle.Bold;
    }

    void Update()
    {
        if (!enabled) return;

        // Pausar durante transiciones
        if (DayNightTransitionManager.Instance != null && DayNightTransitionManager.Instance.IsTransitioning)
            return;

        UpdateFatigue();
    }

    void UpdateFatigue()
    {
        float currentTime = Time.time;
        float timeSinceLastUpdate = currentTime - lastFatigueUpdateTime;

        if (timeSinceLastUpdate >= 1f)
        {
            float fatigueRate = CalculateFatigueRate();

            // Aumentar fatiga si es de noche
            if (gameClock != null && gameClock.IsNightScene)
            {
                fatigueRate *= 1.5f;
            }

            float fatigueThisSecond = fatigueRate * timeSinceLastUpdate;
            currentSleepQuality = Mathf.Clamp(currentSleepQuality - fatigueThisSecond, 0f, maxSleepQuality);
            lastFatigueUpdateTime = currentTime;
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

    void OnGUI()
    {
        if (!enabled) return;
        if (DayNightTransitionManager.Instance != null && DayNightTransitionManager.Instance.IsTransitioning)
            return;

        // Mostrar solo el porcentaje de sueño
        string sleepString = $"Sueño: {currentSleepQuality:F1}%";

        // Color basado en la calidad del sueño
        Color sleepColor = GetSleepColor(currentSleepQuality);
        sleepStyle.normal.textColor = sleepColor;

        // Mostrar en pantalla
        GUI.Label(new Rect(10, 10, 200, 30), sleepString, sleepStyle);
    }

    Color GetSleepColor(float sleepQuality)
    {
        if (sleepQuality >= 80f) return Color.green;
        if (sleepQuality >= 50f) return Color.yellow;
        if (sleepQuality >= 30f) return new Color(1f, 0.5f, 0f); // Naranja
        return Color.red;
    }

    public void Sleep()
    {
        if (gameClock != null)
        {
            int hourSlept = gameClock.CurrentHour;
            currentSleepQuality = CalculateSleepQuality(hourSlept);
            Debug.Log($"💤 SleepSystem: Durmiendo a las {hourSlept}:00 - Calidad: {currentSleepQuality:F1}%");
        }
        else
        {
            currentSleepQuality = CalculateSleepQuality(22);
        }
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
}