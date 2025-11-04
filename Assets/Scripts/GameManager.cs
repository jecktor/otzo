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

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📁 Escena cargada: {scene.name}");

        if (scene.name == "MainMenu")
        {
            CurrentState = GameState.MainMenu;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMainMenu();

            // Mostrar cursor en menú principal
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (scene.name == "SampleScene" || scene.name == "room")
        {
            CurrentState = GameState.Playing;
            if (UIManager.Instance != null)
                UIManager.Instance.HideAllMenus();

            // Ocultar cursor en juego
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void StartGame()
    {
        Debug.Log("🎮 StartGame() - Cargando SampleScene");
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        // OCULTAR CURSOR al empezar el juego
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("🎯 Cursor ocultado para modo juego");

        SceneManager.LoadScene("SampleScene");
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
        Debug.Log("⏸️ Pausando juego (sin detener el tiempo)");
        CurrentState = GameState.Paused;
        // NO pausamos el tiempo del juego - sigue corriendo normalmente
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        Debug.Log("▶️ Reanudando juego");
        CurrentState = GameState.Playing;
        // El tiempo nunca se pausó, así que no necesitamos reanudarlo
        if (UIManager.Instance != null)
            UIManager.Instance.HideAllMenus();
    }

    public void QuitToMainMenu()
    {
        Debug.Log("🏠 Volviendo al menú principal");
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("👋 Saliendo del juego");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}