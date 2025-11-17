using EasyPeasyFirstPersonController;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameClock : MonoBehaviour
{
    public float dayDurationInMinutes = 10f;
    public int startHour = 8;
    public int startDay = 1;
    public float nightClockSlowdown = 0.5f;

    private float gameTime;
    private int currentHour;
    private int currentMinute;
    private int currentDay;
    private float clockTimeMultiplier = 1f;
    private bool fivePMTriggered = false;
    private bool isNightScene = false;
    private int lastCheckedHour = -1;

    public event Action<int, int, int> OnTimeChanged;
    public event Action<int> OnNewDayStarted;
    public event Action OnFivePMReached;

    private GUIStyle clockStyle;
    private GUIStyle sleepStyle;

    public int CurrentHour => currentHour;
    public int CurrentMinute => currentMinute;
    public int CurrentDay => currentDay;
    public bool IsNightScene => isNightScene;
    public string CurrentTimeString => $"{currentHour:00}:{currentMinute:00}";
    public string FullTimeString => $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";

    void Start()
    {
        currentDay = PlayerPrefs.GetInt("CurrentDay", startDay);
        gameTime = 0f;
        CalculateGameTime();

        CreateGUIStyles();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(CleanupAfterSceneLoad());
    }

    System.Collections.IEnumerator CleanupAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        CleanupDuplicateEventSystems();
    }

    void CreateGUIStyles()
    {
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

    void InitializeClock()
    {
        currentHour = startHour;
        currentMinute = 0;
        currentDay = startDay;
        gameTime = 0f;
        fivePMTriggered = false;
        isNightScene = false;
        lastCheckedHour = -1;
        clockTimeMultiplier = 1f;
    }

    void Update()
    {
        if (!enabled) return;

        float deltaTime = Time.deltaTime;
        gameTime += deltaTime * clockTimeMultiplier;

        CalculateGameTime();
        CheckFor5PM();
        CheckForNewDay();

        OnTimeChanged?.Invoke(currentDay, currentHour, currentMinute);
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
            OnNewDayStarted?.Invoke(currentDay);
        }
        lastCheckedHour = currentHour;
    }

    void CheckFor5PM()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ForceDayEndTransition();
        }

        if (currentHour == 17 && currentMinute == 0 && !fivePMTriggered)
        {
            fivePMTriggered = true;
            OnFivePMReached?.Invoke();
            ChangeToNightScene();
        }

        if (currentHour == 17 && currentMinute == 1 && fivePMTriggered)
        {
            fivePMTriggered = false;
        }
    }

    void OnGUI()
    {
        if (!enabled) return;

        string timeString = $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";
        GUI.Label(new Rect(Screen.width - 300, 10, 290, 30), timeString, clockStyle);

        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            float sleepQuality = GameManagerPersistent.Instance.sleepSystem.CurrentSleepQuality;
            string sleepString = $"Sueño: {sleepQuality:F1}%";

            Color sleepColor = GetSleepColor(sleepQuality);
            sleepStyle.normal.textColor = sleepColor;

            GUI.Label(new Rect(10, 10, 200, 30), sleepString, sleepStyle);
        }
    }

    Color GetSleepColor(float sleepQuality)
    {
        if (sleepQuality >= 80f) return Color.green;
        if (sleepQuality >= 50f) return Color.yellow;
        if (sleepQuality >= 30f) return new Color(1f, 0.5f, 0f);
        return Color.red;
    }

    public void SleepAndAdvanceTime()
    {
        currentDay++;
        SetExactTime(8, 0);

        if (IsNightScene)
        {
            ChangeToDayScene();
        }

        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.Save();
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

    public void ChangeToNightScene()
    {
        if (!enabled) return;

        ReEnablePlayerControl();

        string nightScene = "room";
        if (IsSceneInBuildSettings(nightScene))
        {
            isNightScene = true;
            clockTimeMultiplier = nightClockSlowdown;
            UnityEngine.SceneManagement.SceneManager.LoadScene(nightScene);
        }
    }

    public void ChangeToDayScene()
    {
        if (!enabled) return;
        ReEnablePlayerControl();

        string dayScene = "SampleScene";
        if (IsSceneInBuildSettings(dayScene))
        {
            isNightScene = false;
            clockTimeMultiplier = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(dayScene);
        }
    }

    private void ReEnablePlayerControl()
    {
        FirstPersonController[] allPlayers = FindObjectsOfType<FirstPersonController>();
        foreach (FirstPersonController player in allPlayers)
        {
            if (player != null)
            {
                player.SetControl(true);
            }
        }

        ScanMiniGame scanGame = FindObjectOfType<ScanMiniGame>();
        if (scanGame != null)
        {
            scanGame.Stop();
        }
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (buildSceneName == sceneName) return true;
        }
        return false;
    }

    void CleanupDuplicateEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

        if (eventSystems.Length > 1)
        {
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Destroy(eventSystems[i].gameObject);
            }
        }
    }

    public void ForceDayEndTransition()
    {
        ChangeToNightScene();
    }

    public void SetNightSpeed() => clockTimeMultiplier = nightClockSlowdown;
    public void SetDaySpeed() => clockTimeMultiplier = 1f;

    public void SubscribeToTimeChanges(Action<int, int, int> callback) => OnTimeChanged += callback;
    public void SubscribeToNewDay(Action<int> callback) => OnNewDayStarted += callback;
    public void SubscribeToFivePM(Action callback) => OnFivePMReached += callback;

    void OnDestroy()
    {
        OnTimeChanged = null;
        OnNewDayStarted = null;
        OnFivePMReached = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}