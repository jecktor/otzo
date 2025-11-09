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

    public GameState CurrentState { get; private set; } = GameState.Playing;

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
}