using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameClock : MonoBehaviour
{
    public static GameClock Instance;

    [Header("Configuración Tiempo")]
    public float dayDurationInMinutes = 5f;
    public int startHour = 8;
    public int startDay = 1;

    private float gameTime;
    private int currentHour;
    private int currentMinute;
    private int currentDay;
    private GUIStyle clockStyle;
    private bool fivePMTriggered = false;

    public void SleepAndAdvanceTime()
    {
        AdvanceTime();
        ChangeToDayScene();
    }

    public void AdvanceTime()
    {
        float realSecondsPerGameHour = (dayDurationInMinutes * 60f) / 24f;
        float secondsToAdvance = 8 * realSecondsPerGameHour;

        gameTime += secondsToAdvance; // Cambié = por += para sumar tiempo, no resetear
        CalculateGameTime();

        Debug.Log($"Tiempo avanzado 8 horas. Día {currentDay}, Hora: {GetCurrentTimeString()}");
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeClock();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeClock()
    {
        currentHour = startHour;
        currentMinute = 0;
        currentDay = startDay;
        gameTime = 0f;
        fivePMTriggered = false;

        clockStyle = new GUIStyle();
        clockStyle.fontSize = 20;
        clockStyle.normal.textColor = Color.white;
        clockStyle.alignment = TextAnchor.UpperRight;
        clockStyle.fontStyle = FontStyle.Bold;
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        CalculateGameTime();
        CheckFor5PM();
        CheckForNewDay();
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
        int lastCheckedHour = -1;

        if (currentHour == 0 && lastCheckedHour == 23)
        {
            currentDay++;
            OnNewDayStarted();
        }

        lastCheckedHour = currentHour;
    }

    void CheckFor5PM()
    {
        if (currentHour == 17 && currentMinute == 0 && !fivePMTriggered)
        {
            fivePMTriggered = true;
            ChangeToNightScene();
        }

        if (currentHour == 17 && currentMinute == 1 && fivePMTriggered)
        {
            fivePMTriggered = false;
        }

        if (currentHour == 7 && currentMinute == 59)
        {
            ChangeToDayScene();
        }
    }

    void ChangeToNightScene()
    {
        string nightScene = "room";

        if (IsSceneInBuildSettings(nightScene))
        {
            Debug.Log($"Cambiando a escena nocturna: {nightScene}");
            SceneManager.LoadScene(nightScene);
        }
        else
        {
            Debug.LogError($"Escena '{nightScene}' no encontrada en Build Settings");
        }
    }

    public void ChangeToDayScene()
    {
        string dayScene = "SampleScene";

        if (IsSceneInBuildSettings(dayScene))
        {
            Debug.Log($"Cambiando a escena diurna: {dayScene}");
            SceneManager.LoadScene(dayScene);
        }
        else
        {
            Debug.LogError($"Escena '{dayScene}' no encontrada en Build Settings");
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
        string timeString = $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";
        GUI.Label(new Rect(Screen.width - 200, 10, 190, 30), timeString, clockStyle);
    }

    void OnNewDayStarted()
    {
    }

    public int GetCurrentHour() => currentHour;
    public int GetCurrentMinute() => currentMinute;
    public int GetCurrentDay() => currentDay;
    public string GetCurrentTimeString() => $"{currentHour:00}:{currentMinute:00}";
    public string GetFullTimeString() => $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";

    public void SetDay(int day)
    {
        if (day >= startDay)
        {
            currentDay = day;
            Debug.Log($"Día establecido a: {currentDay}");
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
        }
    }
}