using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameClock : MonoBehaviour
{
    public static GameClock Instance;

    [Header("Configuración Tiempo")]
    public float dayDurationInMinutes = 1f;
    public int startHour = 16;
    public int startDay = 1;
    public float nightClockSlowdown = 0.5f;

    [Header("Configuración Sueño")]
    public float maxSleepQuality = 100f;
    public float sleepDeprivationPenalty = 2f;
    public int optimalSleepHour = 22;
    public float baseFatigueRate = 0.5f;
    public float fatigueMultiplier = 2f;

    private float gameTime;
    private int currentHour;
    private int currentMinute;
    private int currentDay;
    private float currentSleepQuality;
    private GUIStyle clockStyle;
    private GUIStyle sleepStyle;
    private bool fivePMTriggered = false;
    private bool isNightScene = false;
    private int lastCheckedHour = -1;
    private float fatigueAccumulator = 0f;
    private float lastFatigueUpdateTime = 0f;
    private float clockTimeMultiplier = 1f;

    public float CurrentSleepQuality => currentSleepQuality;
    public float SleepQualityPercent => currentSleepQuality / maxSleepQuality;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeClock();
            EnsureTransitionManagerExists();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void EnsureTransitionManagerExists()
    {
        DayNightTransitionManager existingManager = FindFirstObjectByType<DayNightTransitionManager>();

        if (existingManager == null)
        {
            GameObject managerObj = new GameObject("DayNightTransitionManager");
            managerObj.AddComponent<DayNightTransitionManager>();
            Debug.Log("DayNightTransitionManager creado automáticamente");
        }
    }

    void InitializeClock()
    {
        currentHour = startHour;
        currentMinute = 0;
        currentDay = startDay;
        currentSleepQuality = maxSleepQuality;
        gameTime = 0f;
        fivePMTriggered = false;
        isNightScene = false;
        lastCheckedHour = -1;
        lastFatigueUpdateTime = Time.time;
        clockTimeMultiplier = 1f;

        clockStyle = new GUIStyle();
        clockStyle.fontSize = 20;
        clockStyle.normal.textColor = Color.white;
        clockStyle.alignment = TextAnchor.UpperRight;
        clockStyle.fontStyle = FontStyle.Bold;

        sleepStyle = new GUIStyle();
        sleepStyle.fontSize = 16;
        sleepStyle.normal.textColor = Color.white;
        sleepStyle.alignment = TextAnchor.UpperLeft;
        sleepStyle.fontStyle = FontStyle.Bold;
    }

    public void SetExactTime(int targetHour, int targetMinute)
    {
        float realSecondsPerGameHour = (dayDurationInMinutes * 60f) / 24f;
        float realSecondsPerGameMinute = realSecondsPerGameHour / 60f;

        int totalMinutesFromStart = (targetHour - startHour) * 60 + targetMinute;
        if (totalMinutesFromStart < 0) totalMinutesFromStart += 24 * 60;

        int daysFromStart = currentDay - startDay;
        float totalGameTime = (daysFromStart * 24 * 60 + totalMinutesFromStart) * realSecondsPerGameMinute;

        gameTime = totalGameTime;
        CalculateGameTime();
    }

    public void SleepAndAdvanceTime()
    {
        CalculateSleepQuality();
        currentDay++;
        SetExactTime(8, 0);
        ChangeToDayScene();
    }

    void CalculateSleepQuality()
    {
        float sleepPenalty = 0f;
        int hourToSleep = currentHour;

        if (hourToSleep < optimalSleepHour)
        {
            int hoursBeforeOptimal = optimalSleepHour - hourToSleep;
            sleepPenalty = hoursBeforeOptimal * sleepDeprivationPenalty;
        }
        else
        {
            int hoursAfterOptimal = hourToSleep - optimalSleepHour;
            sleepPenalty = hoursAfterOptimal * sleepDeprivationPenalty * 2f;
        }

        currentSleepQuality = Mathf.Clamp(maxSleepQuality - sleepPenalty, 10f, maxSleepQuality);
    }

    void UpdateFatigue()
    {
        if (DayNightTransitionManager.Instance != null && DayNightTransitionManager.Instance.IsTransitioning)
            return;

        float currentTime = Time.time;
        float timeSinceLastUpdate = currentTime - lastFatigueUpdateTime;

        if (timeSinceLastUpdate >= 1f)
        {
            float fatigueRate = CalculateFatigueRate();

            if (isNightScene)
            {
                fatigueRate *= 1.5f;
            }

            float fatigueThisSecond = fatigueRate * timeSinceLastUpdate;

            currentSleepQuality = Mathf.Clamp(currentSleepQuality - fatigueThisSecond, 0f, maxSleepQuality);
            lastFatigueUpdateTime = currentTime;

            fatigueAccumulator += fatigueThisSecond;

            if (fatigueAccumulator >= 1f)
            {
                fatigueAccumulator = 0f;
            }
        }
    }

    float CalculateFatigueRate()
    {
        float sleepPercent = SleepQualityPercent;
        float fatigueRate = baseFatigueRate;

        if (sleepPercent >= 0.8f)
        {
            fatigueRate *= 0.2f;
        }
        else if (sleepPercent >= 0.6f)
        {
            fatigueRate *= 0.5f;
        }
        else if (sleepPercent >= 0.4f)
        {
            fatigueRate *= 0.8f;
        }
        else if (sleepPercent >= 0.2f)
        {
            fatigueRate *= 1.5f;
        }
        else
        {
            fatigueRate *= 2f;
        }

        return fatigueRate;
    }

    void Update()
    {
        if (DayNightTransitionManager.Instance != null && DayNightTransitionManager.Instance.IsTransitioning)
            return;

        float deltaTime = Time.deltaTime;
        gameTime += deltaTime * clockTimeMultiplier;

        CalculateGameTime();
        CheckFor5PM();
        CheckForNewDay();
        UpdateFatigue();
    }

    void CalculateGameTime()
    {
        float realSecondsPerGameHour = (dayDurationInMinutes * 60f) / 24f;
        float totalGameHours = gameTime / realSecondsPerGameHour;

        int totalGameMinutes = Mathf.FloorToInt(totalGameHours * 60f);

        int totalHoursFromStart = totalGameMinutes / 60;
        currentDay = startDay + (totalHoursFromStart / 24);
        currentHour = (startHour + totalHoursFromStart) % 24;
        currentMinute = totalGameMinutes % 60;
    }

    void CheckForNewDay()
    {
        if (currentHour == 0 && lastCheckedHour == 23)
        {
            currentDay++;
            OnNewDayStarted();
        }

        lastCheckedHour = currentHour;
    }

    void CheckFor5PM()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Transición forzada con tecla T");
            ForceDayEndTransition();
        }

        if (currentHour == 17 && currentMinute == 0 && !fivePMTriggered)
        {
            fivePMTriggered = true;
            Debug.Log("¡5 PM detectado! Iniciando transición...");

            if (DayNightTransitionManager.Instance != null)
            {
                DayNightTransitionManager.Instance.StartDayEndTransition();
            }
            else
            {
                Debug.LogError("DayNightTransitionManager.Instance es null");
            }
        }

        if (currentHour == 17 && currentMinute == 1 && fivePMTriggered)
        {
            fivePMTriggered = false;
        }
    }

    public void ChangeToNightScene()
    {
        string nightScene = "room";
        if (IsSceneInBuildSettings(nightScene))
        {
            isNightScene = true;
            clockTimeMultiplier = nightClockSlowdown;
            SceneManager.LoadScene(nightScene);
            Debug.Log("Cambiando a escena nocturna: " + nightScene);
        }
        else
        {
            Debug.LogError($"Escena nocturna '{nightScene}' no encontrada en build settings");
        }
    }

    public void ChangeToDayScene()
    {
        string dayScene = "SampleScene";
        if (IsSceneInBuildSettings(dayScene))
        {
            isNightScene = false;
            clockTimeMultiplier = 1f;
            SceneManager.LoadScene(dayScene);
            Debug.Log("Cambiando a escena diurna: " + dayScene);
        }
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (buildSceneName == sceneName)
                return true;
        }
        return false;
    }

    void OnGUI()
    {
        bool isTransitioning = DayNightTransitionManager.Instance != null && DayNightTransitionManager.Instance.IsTransitioning;

        if (!isTransitioning)
        {
            string timeString = $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";
            if (isNightScene)
            {
                timeString += $" (Noche - Reloj {nightClockSlowdown}x)";
            }
            GUI.Label(new Rect(Screen.width - 300, 10, 290, 30), timeString, clockStyle);

            string sleepString = $"Sueño: {currentSleepQuality:F1}%";
            Color sleepColor = GetSleepColor();
            sleepStyle.normal.textColor = sleepColor;
            GUI.Label(new Rect(10, 10, 200, 30), sleepString, sleepStyle);
        }

        if (DayNightTransitionManager.Instance == null)
        {
            GUI.Label(new Rect(10, 50, 400, 30), "ERROR: DayNightTransitionManager no encontrado", clockStyle);
        }
    }

    Color GetSleepColor()
    {
        if (currentSleepQuality >= 80f) return Color.green;
        if (currentSleepQuality >= 50f) return Color.yellow;
        if (currentSleepQuality >= 30f) return new Color(1f, 0.5f, 0f);
        return Color.red;
    }

    void OnNewDayStarted()
    {
        Debug.Log($"Evento: Comenzó el día {currentDay}");
    }

    public void ForceDayEndTransition()
    {
        if (DayNightTransitionManager.Instance != null)
        {
            DayNightTransitionManager.Instance.StartDayEndTransition();
        }
        else
        {
            Debug.LogError("No se puede forzar transición: DayNightTransitionManager no disponible");
        }
    }

    public int GetCurrentHour() => currentHour;
    public int GetCurrentMinute() => currentMinute;
    public int GetCurrentDay() => currentDay;
    public float GetSleepQuality() => currentSleepQuality;
    public float GetSleepQualityPercent() => SleepQualityPercent;
    public string GetCurrentTimeString() => $"{currentHour:00}:{currentMinute:00}";
    public string GetFullTimeString() => $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";

    public void SetDay(int day)
    {
        if (day >= startDay)
        {
            currentDay = day;
        }
    }

    public void ModifySleepQuality(float amount)
    {
        currentSleepQuality = Mathf.Clamp(currentSleepQuality + amount, 0f, maxSleepQuality);
    }

    public void SetSleepQuality(float quality)
    {
        currentSleepQuality = Mathf.Clamp(quality, 0f, maxSleepQuality);
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

    void OnDestroy()
    {
        if (Instance == this)
        {
            // Limpiar si es necesario
        }
    }
}