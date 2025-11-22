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

    // Propiedad para verificar si el reloj está pausado
    public bool IsPaused => DayNightTransitionManager.Instance != null &&
                           DayNightTransitionManager.Instance.IsTransitioning;

    // Tamaños base para 1080p que se escalarán automáticamente
    private int baseFontSizeClock = 24;
    private int baseFontSizeSleep = 20;
    private Vector2 baseScreenReference = new Vector2(1920, 1080);

    void Start()
    {
        currentDay = PlayerPrefs.GetInt("CurrentDay", startDay);
        gameTime = 0f;
        CalculateGameTime();

        CreateGUIStyles();
        EnsureTransitionManagerExists();
        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"🕒 GameClock iniciado - Día {currentDay}");
    }

    void EnsureTransitionManagerExists()
    {
        if (DayNightTransitionManager.Instance == null)
        {
            GameObject transitionManagerObj = new GameObject("DayNightTransitionManager");
            transitionManagerObj.AddComponent<DayNightTransitionManager>();
            Debug.Log("🔄 DayNightTransitionManager creado automáticamente");
        }
        else
        {
            Debug.Log("✅ DayNightTransitionManager ya existe");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(CleanupAfterSceneLoad());
    }

    IEnumerator CleanupAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        CleanupDuplicateEventSystems();
    }

    void CreateGUIStyles()
    {
        // Calcular escala basada en la resolución actual
        float scaleFactor = CalculateScaleFactor();

        // Estilo para el reloj
        clockStyle = new GUIStyle();
        clockStyle.fontSize = Mathf.RoundToInt(baseFontSizeClock * scaleFactor);
        clockStyle.normal.textColor = Color.white;
        clockStyle.alignment = TextAnchor.UpperRight;
        clockStyle.fontStyle = FontStyle.Bold;

        // Estilo para el sueño
        sleepStyle = new GUIStyle();
        sleepStyle.fontSize = Mathf.RoundToInt(baseFontSizeSleep * scaleFactor);
        sleepStyle.normal.textColor = Color.white;
        sleepStyle.alignment = TextAnchor.UpperLeft;
        sleepStyle.fontStyle = FontStyle.Bold;

        Debug.Log($"📐 Escala GUI calculada: {scaleFactor:F2} (Resolución: {Screen.width}x{Screen.height})");
    }

    float CalculateScaleFactor()
    {
        // Calcular escala basada en la altura de la pantalla (más consistente que el ancho)
        float referenceHeight = baseScreenReference.y;
        float currentHeight = Screen.height;

        // Usar una escala logarítmica suave para mejor adaptación
        float scale = currentHeight / referenceHeight;

        // Limitar la escala para evitar textos demasiado grandes o pequeños
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

        // PAUSAR durante transiciones
        if (IsPaused)
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
        // No verificar las 5 PM durante transiciones
        if (IsPaused) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🔑 Tecla T presionada - Forzando transición");
            ForceDayEndTransition();
        }

        if (currentHour == 17 && currentMinute == 0 && !fivePMTriggered)
        {
            fivePMTriggered = true;
            Debug.Log("🕔 ¡Son las 5 PM! Iniciando transición a casa");
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

        // Recalcular estilos en cada frame para adaptarse a cambios de resolución
        CreateGUIStyles();

        string timeString = $"Día {currentDay} - {currentHour:00}:{currentMinute:00}";

        // Mostrar indicador de pausa si está en transición
        if (IsPaused)
        {
            timeString += " ⏸️";
        }

        // Calcular posición y tamaño responsivos
        float padding = GetScaledValue(20);
        float labelWidth = GetScaledValue(300);
        float labelHeight = GetScaledValue(35);

        // Reloj (esquina superior derecha)
        Rect clockRect = new Rect(Screen.width - labelWidth - padding, padding, labelWidth, labelHeight);
        GUI.Label(clockRect, timeString, clockStyle);

        // Sueño (esquina superior izquierda)
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
        Debug.Log("🛌 Método SleepAndAdvanceTime llamado");

        // Obtener calidad de sueño para el mensaje
        float sleepQuality = 50f;
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            sleepQuality = GameManagerPersistent.Instance.sleepSystem.CurrentSleepQuality;
        }

        Debug.Log($"😴 Durmiendo... Día actual: {currentDay}, Calidad de sueño: {sleepQuality}");

        // Iniciar transición a la tienda
        if (DayNightTransitionManager.Instance != null)
        {
            Debug.Log("🚀 Llamando a StartTransitionToStore...");
            DayNightTransitionManager.Instance.StartTransitionToStore(sleepQuality);
        }
        else
        {
            Debug.LogError("❌ DayNightTransitionManager.Instance es NULL! Usando fallback");
            // Fallback: cambiar directamente
            ChangeToDayScene();
        }

        currentDay++;
        SetExactTime(8, 0);

        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.Save();

        Debug.Log($"✅ Día avanzado a: {currentDay}");
    }

    public void ChangeToNightScene()
    {
        if (!enabled) return;

        Debug.Log("🌙 ChangeToNightScene llamado");

        float dailyEarnings = CalculateDailyEarnings();
        Debug.Log($"💰 Ganancias del día calculadas: ${dailyEarnings}");

        if (DayNightTransitionManager.Instance != null)
        {
            Debug.Log("🚀 Llamando a StartTransitionToHome...");
            DayNightTransitionManager.Instance.StartTransitionToHome(dailyEarnings);
        }
        else
        {
            Debug.LogError("❌ DayNightTransitionManager.Instance es NULL! Usando fallback directo");
            string nightScene = "room";
            if (IsSceneInBuildSettings(nightScene))
            {
                isNightScene = true;
                clockTimeMultiplier = nightClockSlowdown;
                UnityEngine.SceneManagement.SceneManager.LoadScene(nightScene);
            }
        }
    }

    private float CalculateDailyEarnings()
    {
        // Ejemplo simple - reemplazar con tu lógica real
        float baseEarnings = 100f;
        float randomBonus = UnityEngine.Random.Range(-30f, 100f);

        // Bonus por mejoras de tienda compradas
        float shopBonus = 0f;
        if (ShopManager.Instance != null && ShopManager.Instance.IsUpgradePurchased("🌟 Mejorar Actitud Clientes"))
        {
            shopBonus = 50f;
        }

        float total = baseEarnings + randomBonus + shopBonus;
        Debug.Log($"💰 Cálculo de ganancias: Base={baseEarnings}, Bonus={randomBonus}, Shop={shopBonus}, Total={total}");
        return total;
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

    public void ChangeToDayScene()
    {
        if (!enabled) return;

        Debug.Log("☀️ ChangeToDayScene llamado (fallback)");
        ReEnablePlayerControl();

        string dayScene = "SampleScene";
        if (IsSceneInBuildSettings(dayScene))
        {
            isNightScene = false;
            clockTimeMultiplier = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(dayScene);
            Debug.Log("✅ Cambiado a escena de día directamente");
        }
        else
        {
            Debug.LogError($"❌ Escena '{dayScene}' no encontrada en build settings");
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
        Debug.Log("🔑 Transición forzada a casa");
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