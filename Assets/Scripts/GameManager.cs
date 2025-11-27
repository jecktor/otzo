using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused
    }

	[Header("UI")]
	public GameObject Prompt;

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    private GameDataManager dataManager;
    private DayNightTransitionManager transitionManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // **IMPORTANTE: NO usar DontDestroyOnLoad para GameManager**
            // DontDestroyOnLoad(gameObject);

            InitializeDataManager();
            InitializeTransitionManager();

            Debug.Log("✅ GameManager inicializado");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Configurar la escena actual
        SetupCurrentScene();
    }

    void SetupCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"🔄 Configurando escena: {currentScene}");

        if (currentScene == "MainMenu")
        {
            SetupMainMenu();
        }
        else if (currentScene == "room" || currentScene == "SampleScene" || currentScene == "endless")
        {
            SetupGameScene();
        }
    }

    void SetupMainMenu()
    {
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;

        // **CONFIGURACIÓN CRÍTICA PARA MENÚ PRINCIPAL**
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

	    Debug.Log("🏠 Menú principal configurado - Cursor visible");
	    if (!IsUserLoggedIn())
	    {
		    Prompt.SetActive(true);
	    }
    }

    void SetupGameScene()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        // **CONFIGURACIÓN PARA JUEGO**
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("🎮 Escena de juego configurada - Cursor oculto");
    }

    void InitializeDataManager()
    {
        dataManager = FindObjectOfType<GameDataManager>();
        if (dataManager == null)
        {
            GameObject dataManagerObj = new GameObject("GameDataManager");
            dataManager = dataManagerObj.AddComponent<GameDataManager>();
            // **IMPORTANTE: GameDataManager SÍ usa DontDestroyOnLoad**
            DontDestroyOnLoad(dataManagerObj);
        }
    }

    void InitializeTransitionManager()
    {
        transitionManager = FindObjectOfType<DayNightTransitionManager>();
        if (transitionManager == null)
        {
            GameObject transitionObj = new GameObject("DayNightTransitionManager");
            transitionManager = transitionObj.AddComponent<DayNightTransitionManager>();
            DontDestroyOnLoad(transitionObj);
            Debug.Log("✅ DayNightTransitionManager creado automáticamente");
        }
    }

    // Función para verificar si hay usuario logeado
    private bool IsUserLoggedIn()
    {
        if (GlobalUser.Instance == null || string.IsNullOrEmpty(GlobalUser.Instance.Username))
        {
            Debug.LogWarning("⚠️ No hay usuario logeado. Inicia sesión primero.");
            return false;
        }
        return true;
    }
    
	public void ClosePrompt()
	{
		Prompt.SetActive(false);
	}

    public async void StartGame()
    {
        // Verificar si hay usuario logeado
        if (!IsUserLoggedIn())
        {
        	Prompt.SetActive(true);
            Debug.LogError("❌ No se puede iniciar juego: No hay usuario logeado");
            return;
        }

        Debug.Log("🎮 StartGame() - Iniciando nuevo juego");

        // **LLAMAR AQUÍ: Resetear TODOS los managers**
        ResetAllManagers();

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("🎯 Cursor ocultado para modo juego");

        await ResetGameData();

        // **NUEVO: Resetear flags de transición para JUEGO NUEVO**
        ResetTransitionFlags(true); // ← true indica juego nuevo

        // **DEBUG: Verificar estado después del reset**
        DebugTransitions();

        // Asegurarse de que el transitionManager esté disponible
        if (transitionManager == null)
        {
            InitializeTransitionManager();
        }

        if (transitionManager != null && !transitionManager.IsTransitioning)
        {
            Debug.Log("🎬 Iniciando transición con imagen de intro");
            transitionManager.StartTransitionWithIntroImage("room");
        }
        else
        {
            Debug.LogWarning("⚠️ TransitionManager no disponible, cargando escena directamente");
            SceneManager.LoadScene("room");
        }
    }
    [ContextMenu("Debug Transiciones")]
    public void DebugTransitions()
    {
        if (DayNightTransitionManager.Instance != null)
        {
            Debug.Log("=== DEBUG TRANSICIONES ===");
            Debug.Log($"🎬 RoomToStore: {DayNightTransitionManager.Instance.HasShownRoomToStore}");
            Debug.Log($"🎬 StoreToHome: {DayNightTransitionManager.Instance.HasShownStoreToHome}");
            Debug.Log($"🎬 Intro: {DayNightTransitionManager.Instance.HasShownIntro}");

            int currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
            bool hasCompletedCycle = PlayerPrefs.GetInt("HasCompletedFirstCycle", 0) == 1;
            Debug.Log($"📊 Datos - Día: {currentDay}, CicloCompletado: {hasCompletedCycle}");
        }
    }
    private void ResetTransitionFlags(bool isNewGame = false)
    {
        if (DayNightTransitionManager.Instance != null)
        {
            DayNightTransitionManager.Instance.ResetTransitionFlagsBasedOnGameState(isNewGame);
            Debug.Log($"✅ Flags de transición reseteadas - NuevoJuego: {isNewGame}");
        }
    }


    private void ResetAllManagers()
    {
        ResetGameClock();
        ResetEmailManager();
        ResetShopManager();
        Debug.Log("✅ Todos los managers reseteados");
    }

    private void ResetEmailManager()
    {
        Debug.Log("🔄 Reseteando EmailManager...");

        // Buscar y destruir EmailManager existente
        EmailManager[] existingManagers = FindObjectsOfType<EmailManager>();
        foreach (EmailManager manager in existingManagers)
        {
            if (manager != null && manager.gameObject != null)
            {
                Destroy(manager.gameObject);
            }
        }

        Debug.Log("✅ EmailManager reseteado");
    }

    private void ResetShopManager()
    {
        Debug.Log("🔄 Reseteando ShopManager...");

        // Buscar y destruir ShopManager existente
        ShopManager[] existingManagers = FindObjectsOfType<ShopManager>();
        foreach (ShopManager manager in existingManagers)
        {
            if (manager != null && manager.gameObject != null)
            {
                Destroy(manager.gameObject);
            }
        }

        Debug.Log("✅ ShopManager reseteado");
    }

    private void ResetLocalData()
    {
        Debug.Log("🗑️ Reseteando datos locales...");

        PlayerPrefs.DeleteKey("PlayerMoney");
        PlayerPrefs.DeleteKey("SleepQuality");
        PlayerPrefs.SetInt("CurrentDay", 1);
        PlayerPrefs.SetInt("LastSavedDay", 1);
        PlayerPrefs.SetInt("HasCompletedFirstCycle", 0);
        PlayerPrefs.DeleteKey("PurchasedUpgrades");
        PlayerPrefs.DeleteKey("BestScore");
        PlayerPrefs.DeleteKey("HasReadFirstDayEmail");
        PlayerPrefs.Save();

        Debug.Log("✅ Datos locales reseteados - Día 1, ciclo NO completado");
    }

    private void ResetGameClock()
    {
        if (GameClock.Instance != null)
        {
            GameClock.Instance.ForceResetToDayOne();
        }
        else
        {
            Debug.Log("ℹ️ No hay GameClock activo para resetear");
        }
    }
    private async Task ResetGameData()
    {
        if (!IsUserLoggedIn())
        {
            Debug.LogError("❌ No se puede resetear datos: No hay usuario logeado");
            return;
        }

        Debug.Log("🔄 Reseteando datos del juego...");

        try
        {
            ResetLocalData();
            await ResetFirebaseData();
            Debug.Log("✅ Datos reseteados exitosamente");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error reseteando datos: {ex.Message}");
        }
    }

    public async void StartArcade()
    {
        // Verificar si hay usuario logeado
        if (!IsUserLoggedIn())
        {
        	Prompt.SetActive(true);
            Debug.LogError("❌ No se puede iniciar arcade: No hay usuario logeado");
            return;
        }

        Debug.Log("🎮 StartArcade() - Iniciando modo arcade");
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("🎯 Cursor ocultado para modo juego");

        await ResetGameData();

        SceneManager.LoadScene("endless");
    }

    public async void LoadRoomWithFirebaseData()
    {
        // Verificar si hay usuario logeado
        if (!IsUserLoggedIn())
        {
        	Prompt.SetActive(true);
            Debug.LogError("❌ No se puede cargar room: No hay usuario logeado");
            return;
        }

        Debug.Log("🏠 LoadRoomWithFirebaseData() - Cargando room con datos de Firebase");

        try
        {
            await LoadDataFromFirebase();

            // **NUEVO: Forzar estado correcto del GameClock después de cargar**
            ForceCorrectGameClockState();

            // **NUEVO: Resetear flags de transición para CONTINUAR partida**
            ResetTransitionFlags(false); // ← false indica continuar partida

            CurrentState = GameState.Playing;
            Time.timeScale = 1f;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            SceneManager.LoadScene("room");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error cargando datos de Firebase: {ex.Message}");
            SceneManager.LoadScene("room");
        }
    }

    // **NUEVO MÉTODO: Resetear flags de transición**


    // **NUEVO MÉTODO: Corregir estado del GameClock**
    // **MEJORADO: Corregir estado del GameClock**
    private void ForceCorrectGameClockState()
    {
        if (GameClock.Instance != null)
        {
            int savedDay = PlayerPrefs.GetInt("CurrentDay", 1);
            bool hasCompletedCycle = PlayerPrefs.GetInt("HasCompletedFirstCycle", 0) == 1;

            Debug.Log($"🔧 Forzando estado GameClock - Día: {savedDay}, CicloCompletado: {hasCompletedCycle}");

            // Si el día es 1 PERO ya completamos el ciclo, no es primer día
            if (savedDay == 1 && hasCompletedCycle)
            {
                GameClock.Instance.CompleteFirstDay();
                Debug.Log("🔄 GameClock forzado: Día 1 pero NO es primer día (ciclo completado)");
            }

            // **NUEVO: También resetear el día si es mayor a 1**
            else if (savedDay > 1)
            {
                GameClock.Instance.CompleteFirstDay();
                Debug.Log($"🔄 GameClock forzado: Día {savedDay} - No es primer día");
            }
        }
    }
    private async Task LoadDataFromFirebase()
    {
        Debug.Log("📥 Cargando datos desde Firebase...");

        if (dataManager != null)
        {
            dataManager.LoadUserDataFromFirebase();
            await Task.Delay(1000);
        }
        else
        {
            Debug.LogWarning("⚠️ GameDataManager no disponible");
        }
    }

    private async Task ResetFirebaseData()
    {
        Debug.Log("☁️ Reseteando datos en Firebase...");

        if (dataManager != null)
        {
            float defaultMoney = 0f;
            float defaultSleep = 100f;
            int defaultDay = 1;
            string defaultUpgrades = "";

            PlayerPrefs.SetFloat("PlayerMoney", defaultMoney);
            PlayerPrefs.SetFloat("SleepQuality", defaultSleep);
            PlayerPrefs.SetInt("CurrentDay", defaultDay);
            PlayerPrefs.SetString("PurchasedUpgrades", defaultUpgrades);
            PlayerPrefs.SetInt("HasReadFirstDayEmail", 0);
            PlayerPrefs.Save();

            dataManager.SaveUserDataToFirebase();

            await Task.Delay(500);
        }
    }

    private void ResetPersistentManagers()
    {
        var persistentManagers = FindObjectsOfType<MonoBehaviour>();
        foreach (var manager in persistentManagers)
        {
            if (manager.gameObject.scene.buildIndex == -1) // Es DontDestroyOnLoad
            {
                var resetMethod = manager.GetType().GetMethod("ResetData");
                if (resetMethod != null)
                {
                    resetMethod.Invoke(manager, null);
                    Debug.Log($"✅ Reseteados datos en: {manager.GetType().Name}");
                }
            }
        }
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (CurrentState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("⏸️ Pausando juego");
        CurrentState = GameState.Paused;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        Debug.Log("▶️ Reanudando juego");
        CurrentState = GameState.Playing;

        if (UIManager.Instance != null)
            UIManager.Instance.HideAllMenus();
    }

    public void QuitToMainMenu()
    {
        Debug.Log("🏠 Volviendo al menú principal");

        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;

        // **IMPORTANTE: Destruir UIManager antes de cambiar de escena**
        if (UIManager.Instance != null)
        {
            Destroy(UIManager.Instance.gameObject);
        }

        // MOSTRAR CURSOR en el menú principal
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("👋 Saliendo del juego");

        // Guardar datos antes de salir (solo si hay usuario logeado)
        if (dataManager != null && IsUserLoggedIn())
        {
            dataManager.SaveUserDataToFirebase();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    [ContextMenu("Forzar Reset Completo")]
    public async void ForceCompleteReset()
    {
        // Verificar si hay usuario logeado
        if (!IsUserLoggedIn())
        {
            Debug.LogError("❌ No se puede forzar reset: No hay usuario logeado");
            return;
        }

        await ResetGameData();
        Debug.Log("🔄 Reset completo forzado ejecutado");
    }

    [ContextMenu("Verificar Estado de Datos")]
    public void CheckDataStatus()
    {
        // Verificar si hay usuario logeado
        if (!IsUserLoggedIn())
        {
            Debug.LogError("❌ No se puede verificar datos: No hay usuario logeado");
            return;
        }

        Debug.Log("=== ESTADO ACTUAL DE DATOS ===");
        Debug.Log($"💰 Dinero: ${PlayerPrefs.GetFloat("PlayerMoney", 2500f):F2}");
        Debug.Log($"😴 Sueño: {PlayerPrefs.GetFloat("SleepQuality", 100f):F1}%");
        Debug.Log($"📅 Día: {PlayerPrefs.GetInt("CurrentDay", 1)}");
        Debug.Log($"🛍️ Mejoras: {PlayerPrefs.GetString("PurchasedUpgrades", "Ninguna")}");

        if (dataManager != null)
        {
            dataManager.ShowCurrentData();
        }
    }

    // Método público para verificar estado de login
    public void CheckLoginStatus()
    {
        if (IsUserLoggedIn())
        {
            Debug.Log($"✅ Usuario logeado: {GlobalUser.Instance.Username}");
        }
        else
        {
            Debug.Log("❌ No hay usuario logeado");
        }
    }

    [ContextMenu("Corregir Día a 1")]
    public void ForceFixDayToOne()
    {
        if (dataManager != null)
        {
            dataManager.ForceFixDayToOne();
        }
    }


}