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

            if (gameClock == null) gameClock = GetComponent<GameClock>();
            if (sleepSystem == null) sleepSystem = GetComponent<SleepSystem>();
            if (uiManager == null) uiManager = GetComponent<UIManager>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSystems()
    {
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
        else if (scene.name == "endless")
        {
	        if (gameClock != null) gameClock.enabled = false;
	        if (sleepSystem != null) sleepSystem.enabled = false;

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