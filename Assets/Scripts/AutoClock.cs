using EasyPeasyFirstPersonController;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

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
    private bool isFirstDay = true;

    public static GameClock Instance { get; private set; }

    public event Action<int, int, int> OnTimeChanged;
    public event Action<int> OnNewDayStarted;
    public event Action OnFivePMReached;

    private GUIStyle clockStyle;
    private GUIStyle sleepStyle;
    private Texture2D backgroundTex; // ⚠️ NUEVO: Mantener referencia para destruir

    // ⚠️ NUEVO: Flag para saber si los estilos ya están creados
    private bool stylesInitialized = false;
    private int lastScreenWidth = 0;
    private int lastScreenHeight = 0;

    public int CurrentHour => currentHour;
    public int CurrentMinute => currentMinute;
    public int CurrentDay => currentDay;
    public bool IsNightScene => isNightScene;
    public string CurrentTimeString => $"{currentHour:00}:{currentMinute:00}";
    public string FullTimeString => $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";

    public bool IsPaused => DayNightTransitionManager.Instance != null &&
                           DayNightTransitionManager.Instance.IsTransitioning;

    private int baseFontSizeClock = 24;
    private int baseFontSizeSleep = 20;
    private Vector2 baseScreenReference = new Vector2(1920, 1080);

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
            return;
        }
    }

    void Start()
    {
        if (currentDay == 0)
        {
            currentDay = PlayerPrefs.GetInt("CurrentDay", startDay);
        }

        Debug.Log($"🔍 DEBUG GameClock Start - Escena: {SceneManager.GetActiveScene().name}");
        Debug.Log($"🔍 PlayerPrefs CurrentDay: {PlayerPrefs.GetInt("CurrentDay", -1)}");
        Debug.Log($"🔍 currentDay variable: {currentDay}");
        Debug.Log($"🔍 startDay: {startDay}");

        isFirstDay = currentDay == 1;

        Debug.Log($"🕒 GameClock iniciado - Día: {currentDay}, Primer día: {isFirstDay}");

        if (isFirstDay && SceneManager.GetActiveScene().name == "room")
        {
            SetExactTime(21, 0);
            Debug.Log("⏰ Primer día en room - Hora fijada a 21:00");
        }
        else if (isFirstDay && SceneManager.GetActiveScene().name == "SampleScene")
        {
            SetExactTime(8, 0);
            Debug.Log("⏰ Primer día en SampleScene - Hora fijada a 8:00 AM");
        }
        else
        {
            SetExactTime(8, 0);
            Debug.Log("⏰ Día posterior - Hora fijada a 8:00 AM");
        }

        // ⚠️ CRÍTICO: Crear estilos UNA SOLA VEZ al inicio
        InitializeGUIStyles();

        EnsureTransitionManagerExists();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void EnsureTransitionManagerExists()
    {
        if (DayNightTransitionManager.Instance == null)
        {
            GameObject transitionManagerObj = new GameObject("DayNightTransitionManager");
            transitionManagerObj.AddComponent<DayNightTransitionManager>();
            DontDestroyOnLoad(transitionManagerObj);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 GameClock - Escena cargada: {scene.name}, Día actual: {currentDay}");

        StartCoroutine(CleanupAfterSceneLoad());

        if (scene.name == "room")
        {
            SaveDataAtDayEnd();
        }

        ConfigureClockForScene(scene.name);
    }

    void ConfigureClockForScene(string sceneName)
    {
        if (sceneName == "room")
        {
            isNightScene = true;
            clockTimeMultiplier = nightClockSlowdown;
            Debug.Log("🌙 Escena room - Velocidad nocturna");
        }
        else if (sceneName == "SampleScene")
        {
            isNightScene = false;
            clockTimeMultiplier = 1f;
            Debug.Log("☀️ Escena SampleScene - Velocidad normal");
        }
    }

    IEnumerator CleanupAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        CleanupDuplicateEventSystems();
    }

    // ⚠️ NUEVO: Inicializar estilos UNA SOLA VEZ
    void InitializeGUIStyles()
    {
        if (stylesInitialized)
        {
            Debug.LogWarning("⚠️ Intentando recrear estilos que ya existen");
            return;
        }

        float scaleFactor = CalculateScaleFactor();

        clockStyle = new GUIStyle();
        clockStyle.fontSize = Mathf.RoundToInt(baseFontSizeClock * scaleFactor);
        clockStyle.normal.textColor = Color.white;
        clockStyle.alignment = TextAnchor.UpperRight;
        clockStyle.fontStyle = FontStyle.Bold;

        sleepStyle = new GUIStyle();
        sleepStyle.fontSize = Mathf.RoundToInt(baseFontSizeSleep * scaleFactor);
        sleepStyle.normal.textColor = Color.white;
        sleepStyle.alignment = TextAnchor.UpperLeft;
        sleepStyle.fontStyle = FontStyle.Bold;

        // ⚠️ OPCIONAL: Si necesitas background
        // backgroundTex = new Texture2D(1, 1);
        // backgroundTex.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        // backgroundTex.Apply();

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        stylesInitialized = true;

        Debug.Log("✅ GUI Styles inicializados");
    }

    // ⚠️ NUEVO: Solo recrear estilos si cambió la resolución
    void CheckAndUpdateGUIStyles()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            Debug.Log("🔄 Resolución cambió - Recreando estilos");
            stylesInitialized = false;
            InitializeGUIStyles();
        }
    }

    // ⚠️ ELIMINADO: CreateGUIStyles() ya no existe

    float CalculateScaleFactor()
    {
        float referenceHeight = baseScreenReference.y;
        float currentHeight = Screen.height;
        float scale = currentHeight / referenceHeight;
        scale = Mathf.Clamp(scale, 0.5f, 2.0f);
        return scale;
    }

    int GetScaledValue(int baseValue)
    {
        float scaleFactor = CalculateScaleFactor();
        return Mathf.RoundToInt(baseValue * scaleFactor);
    }

    void Update()
    {
        if (!enabled) return;

        if (IsPaused)
        {
            return;
        }

        if (isFirstDay && SceneManager.GetActiveScene().name == "room")
        {
            return;
        }

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

        currentHour = (startHour + totalHoursFromStart) % 24;
        currentMinute = totalGameMinutes % 60;

        if (currentHour == 0 && lastCheckedHour == 23)
        {
            if (!isFirstDay)
            {
                currentDay++;
                Debug.Log($"🌅 Nuevo día automático: Día {currentDay}");
                OnNewDayStarted?.Invoke(currentDay);
            }
        }
        lastCheckedHour = currentHour;
    }

    void CheckForNewDay()
    {
        // Manejado en CalculateGameTime
    }

    void CheckFor5PM()
    {
        if (IsPaused) return;

        if (isFirstDay && SceneManager.GetActiveScene().name == "room") return;

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

        // ⚠️ CRÍTICO: Solo verificar cambios de resolución, NO recrear estilos cada frame
        CheckAndUpdateGUIStyles();

        string timeString;

        if (isFirstDay && SceneManager.GetActiveScene().name == "room")
        {
            timeString = $"{currentHour:00}:{currentMinute:00}";
        }
        else
        {
            timeString = $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";
        }

        if (IsPaused)
        {
            timeString += "";
        }

        float padding = GetScaledValue(20);
        float labelWidth = GetScaledValue(300);
        float labelHeight = GetScaledValue(35);

        Rect clockRect = new Rect(Screen.width - labelWidth - padding, padding, labelWidth, labelHeight);
        GUI.Label(clockRect, timeString, clockStyle);

        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            float sleepQuality = GameManagerPersistent.Instance.sleepSystem.CurrentSleepQuality;
            string sleepString = $"Sueño: {sleepQuality:F1}%";

            Color sleepColor = GetSleepColor(sleepQuality);
            sleepStyle.normal.textColor = sleepColor;

            Rect sleepRect = new Rect(padding, padding, labelWidth, labelHeight);
            GUI.Label(sleepRect, sleepString, sleepStyle);
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
        float sleepQuality = 50f;
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            sleepQuality = GameManagerPersistent.Instance.sleepSystem.CurrentSleepQuality;
        }

        int dayBeforeSleep = currentDay;

        if (isFirstDay)
        {
            isFirstDay = false;
            Debug.Log($"📅 Primer ciclo completado - Manteniendo día 1");
        }
        else
        {
            currentDay++;
            Debug.Log($"📅 Avanzando al día {currentDay}");
        }

        SetExactTime(8, 0);

        //PlayerPrefs.SetInt("CurrentDay", currentDay);
        //PlayerPrefs.Save();

        Debug.Log($"✅ Dormido - Día anterior: {dayBeforeSleep}, Día actual: {currentDay}, Primer día: {isFirstDay}");

        if (dayBeforeSleep == 1 && DayNightTransitionManager.Instance != null)
        {
            DayNightTransitionManager.Instance.StartTransitionToStore(sleepQuality);
            Debug.Log("🎬 Transición con imagen para primer día");
        }
        else
        {
            ChangeToDayScene();
            Debug.Log("🔄 Cambio directo de escena para días normales");
        }
    }

    public void ChangeToNightScene()
    {
        if (!enabled) return;

        float dailyEarnings = CalculateDailyEarnings();

        if (DayNightTransitionManager.Instance != null)
        {
            DayNightTransitionManager.Instance.StartTransitionToHome(dailyEarnings);
        }
        else
        {
            string nightScene = "room";
            if (IsSceneInBuildSettings(nightScene))
            {
                isNightScene = true;
                clockTimeMultiplier = nightClockSlowdown;
                UnityEngine.SceneManagement.SceneManager.LoadScene(nightScene);
            }
        }
    }

    private void SaveDataAtDayEnd()
    {
        if (GameDataManager.Instance != null)
        {
            Debug.Log("🏠 Cargando escena room - Guardando datos en Firebase");
            GameDataManager.Instance.SaveUserDataToFirebase();
        }
        else
        {
            Debug.LogWarning("⚠️ GameDataManager no disponible para guardar al final del día");
        }
    }

    private float CalculateDailyEarnings()
    {
        float baseEarnings = 100f;
        float randomBonus = UnityEngine.Random.Range(-30f, 100f);

        float shopBonus = 0f;
        if (ShopManager.Instance != null && ShopManager.Instance.IsUpgradePurchased("🌟 Mejorar Actitud Clientes"))
        {
            shopBonus = 50f;
        }

        float total = baseEarnings + randomBonus + shopBonus;
        return total;
    }

    public void SetExactTime(int targetHour, int targetMinute)
    {
        float realSecondsPerGameHour = (dayDurationInMinutes * 60f) / 24f;
        float realSecondsPerGameMinute = realSecondsPerGameHour / 60f;

        int totalMinutesFromStart = (targetHour - startHour) * 60 + targetMinute;
        if (totalMinutesFromStart < 0) totalMinutesFromStart += 24 * 60;

        float totalGameTime = totalMinutesFromStart * realSecondsPerGameMinute;

        gameTime = totalGameTime;
        CalculateGameTime();
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
            if (buildSceneName == sceneName)
            {
                return true;
            }
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

    public bool IsFirstDay()
    {
        return isFirstDay;
    }

    public void CompleteFirstDay()
    {
        if (isFirstDay)
        {
            isFirstDay = false;
            Debug.Log("🎯 Primer día completado - Listo para ciclo normal");
        }
    }

    public void SubscribeToTimeChanges(Action<int, int, int> callback)
    {
        OnTimeChanged += callback;
    }

    public void UnsubscribeFromTimeChanges(Action<int, int, int> callback)
    {
        OnTimeChanged -= callback;
    }

    public void SubscribeToNewDay(Action<int> callback)
    {
        OnNewDayStarted += callback;
    }

    public void UnsubscribeFromNewDay(Action<int> callback)
    {
        OnNewDayStarted -= callback;
    }

    public void SubscribeToFivePM(Action callback)
    {
        OnFivePMReached += callback;
    }

    public void UnsubscribeFromFivePM(Action callback)
    {
        OnFivePMReached -= callback;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            if (backgroundTex != null)
            {
                Destroy(backgroundTex);
                backgroundTex = null;
            }

            OnTimeChanged = null;
            OnNewDayStarted = null;
            OnFivePMReached = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            Debug.Log("🧹 GameClock limpiado correctamente");
        }
    }
}