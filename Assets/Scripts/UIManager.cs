using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Paused
    }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private GameObject pauseMenuCanvas;
    private GameObject eventSystem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("🟢 UIManager inicializado (solo pausa)");

            CreateEventSystem();
            CreatePauseMenu();
            HideAllMenus();

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

        // Siempre empezar en estado Playing al cargar una escena de juego
        if (scene.name == "SampleScene" || scene.name == "room")
        {
            CurrentState = GameState.Playing;
            HideAllMenus();
            Time.timeScale = 1f; // Asegurar que el tiempo corra
        }
    }

    void CreateEventSystem()
    {
        EventSystem existingEventSystem = FindObjectOfType<EventSystem>();
        if (existingEventSystem == null)
        {
            eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystem);
        }
        else
        {
            eventSystem = existingEventSystem.gameObject;
            DontDestroyOnLoad(eventSystem);
        }
    }

    void CreatePauseMenu()
    {
        Debug.Log("📋 Creando menú de pausa...");

        // Canvas del menú de pausa con HIGH sort order
        pauseMenuCanvas = CreateCanvas("PauseMenuCanvas", 999); // Número alto para que esté encima de todo
        pauseMenuCanvas.SetActive(false);

        // Fondo semitransparente
        CreateFullScreenImage(pauseMenuCanvas, new Color(0f, 0f, 0f, 0.8f));

        // Título de pausa
        CreateSimpleText(pauseMenuCanvas, "JUEGO EN PAUSA", 56,
                        new Vector2(0.5f, 0.7f), new Vector2(600, 80), Color.white);

        // Botón CONTINUAR
        CreateSimpleButton(pauseMenuCanvas, "CONTINUAR", 42,
                          new Vector2(0.5f, 0.5f), new Vector2(400, 80),
                          () => {
                              Debug.Log("▶️ Botón CONTINUAR clickeado");
                              ResumeGame();
                          });

        // Botón MENÚ PRINCIPAL
        CreateSimpleButton(pauseMenuCanvas, "MENÚ PRINCIPAL", 36,
                          new Vector2(0.5f, 0.35f), new Vector2(400, 80),
                          () => {
                              Debug.Log("🏠 Botón MENÚ PRINCIPAL clickeado");
                              QuitToMainMenu();
                          });

        Debug.Log("✅ Menú de pausa creado");
    }

    GameObject CreateCanvas(string name, int sortOrder)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder; // Alto para que esté encima de todo

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.transform.SetParent(transform);

        return canvasObj;
    }

    void CreateFullScreenImage(GameObject parent, Color color)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(parent.transform);

        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        bg.AddComponent<Image>().color = color;
    }

    void CreateSimpleText(GameObject parent, string text, int fontSize, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = new Vector2(anchoredPosition.x * 2 - 1, anchoredPosition.y * 2 - 1) * 500f;

        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = fontSize;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = color;
        textComp.fontStyle = FontStyle.Bold;
    }

    void CreateSimpleButton(GameObject parent, string text, int fontSize, Vector2 anchoredPosition, Vector2 size, System.Action action)
    {
        GameObject buttonObj = new GameObject("Button_" + text);
        buttonObj.transform.SetParent(parent.transform);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = new Vector2(anchoredPosition.x * 2 - 1, anchoredPosition.y * 2 - 1) * 500f;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.selectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        button.onClick.AddListener(() => {
            Debug.Log($"🟢 Botón '{text}' recibió clic");
            action?.Invoke();
        });

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = fontSize;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;
        textComp.fontStyle = FontStyle.Bold;

        textObj.transform.SetAsLastSibling();
    }

    public void ShowPauseMenu()
    {
        Debug.Log("📱 Mostrando menú de pausa");
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
            Canvas.ForceUpdateCanvases();

            // PAUSAR TODO EL JUEGO
            Time.timeScale = 0f;

            // MOSTRAR Y DESBLOQUEAR EL CURSOR
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("🖱️ Cursor mostrado y desbloqueado para menú de pausa");

            // Desactivar PlayerInput cuando el menú de pausa está activo
            PlayerInput playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
                Debug.Log("🔴 PlayerInput desactivado para menú de pausa");
            }
        }
    }

    public void HideAllMenus()
    {
        Debug.Log("📱 Ocultando todos los menús");
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);

            // REANUDAR EL JUEGO
            Time.timeScale = 1f;

            // OCULTAR Y BLOQUEAR EL CURSOR (solo si estamos en modo juego)
            if (CurrentState == GameState.Playing)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Debug.Log("🖱️ Cursor ocultado y bloqueado para modo juego");
            }

            // Reactivar PlayerInput cuando se cierra el menú
            PlayerInput playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
                Debug.Log("🟢 PlayerInput reactivado");
            }
        }
    }

    // Métodos de gestión del juego
    public void PauseGame()
    {
        Debug.Log("⏸️ Pausando juego");
        CurrentState = GameState.Paused;
        ShowPauseMenu();
    }

    public void ResumeGame()
    {
        Debug.Log("▶️ Reanudando juego");
        CurrentState = GameState.Playing;
        HideAllMenus();
    }

    public void QuitToMainMenu()
    {
        Debug.Log("🏠 Volviendo al menú principal");
        CurrentState = GameState.Playing; // Reset state
        Time.timeScale = 1f; // Asegurar que el tiempo corra
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"⎈ ESC presionado - Estado actual: {CurrentState}");

            if (CurrentState == GameState.Playing)
            {
                Debug.Log("⏸️ Cambiando a estado Paused");
                PauseGame();
            }
            else if (CurrentState == GameState.Paused)
            {
                Debug.Log("▶️ Cambiando a estado Playing");
                ResumeGame();
            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (eventSystem != null)
                Destroy(eventSystem);
        }
    }
}