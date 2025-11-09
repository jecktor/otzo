using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerPersistent : MonoBehaviour
{
    public static GameManagerPersistent Instance { get; private set; }

    [Header("Sistemas Persistentes")]
    public GameClock gameClock;
    public SleepSystem sleepSystem;
    public UIManager uiManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSystems()
    {
        // Obtener referencias automáticamente
        gameClock = GetComponent<GameClock>();
        sleepSystem = GetComponent<SleepSystem>();
        uiManager = GetComponent<UIManager>();

        Debug.Log("✅ GameManagerPersistent inicializado");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📁 Escena cargada: {scene.name}");

        if (scene.name == "MainMenu")
        {
            if (gameClock != null) gameClock.enabled = false;
            if (sleepSystem != null) sleepSystem.enabled = false;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (scene.name == "SampleScene" || scene.name == "room")
        {
            if (gameClock != null) gameClock.enabled = true;
            if (sleepSystem != null) sleepSystem.enabled = true;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}