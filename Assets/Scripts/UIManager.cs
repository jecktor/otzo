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
    private string[] allowedScenes = { "SampleScene", "room", "endless" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

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
        Debug.Log($"🔄 UIManager - Escena cargada: {scene.name}");

        // **CORRECCIÓN CRÍTICA: Limpiar EventSystems duplicados**
        CleanupDuplicateEventSystems();

        // Solo resetear el estado si es una escena permitida
        if (IsSceneAllowed(scene.name))
        {
            CurrentState = GameState.Playing;
            HideAllMenus();
            Time.timeScale = 1f;
        }
        else
        {
            // Si no es una escena permitida, ocultar el menú de pausa
            HideAllMenus();
        }
    }

    // **NUEVO: Método para limpiar EventSystems duplicados**
    void CleanupDuplicateEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        Debug.Log($"🔍 Encontrados {eventSystems.Length} EventSystems en escena {SceneManager.GetActiveScene().name}");

        if (eventSystems.Length > 1)
        {
            // Mantener solo el primer EventSystem activo, destruir los demás
            EventSystem firstEventSystem = eventSystems[0];

            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"🗑️ Destruyendo EventSystem duplicado: {eventSystems[i].gameObject.name}");
                Destroy(eventSystems[i].gameObject);
            }

            // Asegurarse de que el EventSystem restante esté configurado correctamente
            if (firstEventSystem != null)
            {
                firstEventSystem.gameObject.SetActive(true);
                Debug.Log($"✅ EventSystem activo: {firstEventSystem.gameObject.name}");
            }
        }
        else if (eventSystems.Length == 1)
        {
            Debug.Log($"✅ Solo hay un EventSystem: {eventSystems[0].gameObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay EventSystem en la escena");
        }

        // **LIMPIAR TAMBIÉN StandaloneInputModule duplicados**
        StandaloneInputModule[] inputModules = FindObjectsOfType<StandaloneInputModule>();
        if (inputModules.Length > 1)
        {
            for (int i = 1; i < inputModules.Length; i++)
            {
                Debug.Log($"🗑️ Destruyendo StandaloneInputModule duplicado: {inputModules[i].gameObject.name}");
                Destroy(inputModules[i].gameObject);
            }
        }
    }

    bool IsSceneAllowed(string sceneName)
    {
        foreach (string allowedScene in allowedScenes)
        {
            if (sceneName == allowedScene)
                return true;
        }
        return false;
    }

    void CreatePauseMenu()
    {
        pauseMenuCanvas = CreateCanvas("PauseMenuCanvas", 999);
        pauseMenuCanvas.SetActive(false);

        CreateFullScreenImage(pauseMenuCanvas, new Color(0f, 0f, 0f, 0.8f));

        CreateSimpleText(pauseMenuCanvas, "JUEGO EN PAUSA", 56,
                        new Vector2(0.5f, 0.7f), new Vector2(600, 80), Color.white);

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
    }

    GameObject CreateCanvas(string name, int sortOrder)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

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
        // Solo mostrar si estamos en una escena permitida
        string currentScene = SceneManager.GetActiveScene().name;
        if (!IsSceneAllowed(currentScene))
            return;

        Debug.Log("📱 Mostrando menú de pausa");
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
            Canvas.ForceUpdateCanvases();

            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            PlayerInput playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
        }
    }

    public void HideAllMenus()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);

            Time.timeScale = 1f;

            // Solo bloquear cursor si estamos en una escena de juego
            string currentScene = SceneManager.GetActiveScene().name;
            if (IsSceneAllowed(currentScene) && CurrentState == GameState.Playing)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                // En otras escenas (como menú principal), dejar cursor visible
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            PlayerInput playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
        }
    }

    public void PauseGame()
    {
        // Solo pausar si estamos en una escena permitida
        string currentScene = SceneManager.GetActiveScene().name;
        if (!IsSceneAllowed(currentScene))
            return;

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

        // **CORRECCIÓN: Limpiar antes de cambiar de escena**
        CleanupDuplicateEventSystems();

        // Cerrar el menú de pausa inmediatamente
        HideAllMenus();

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        // **IMPORTANTE: Configurar cursor para menú principal**
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Cargar el menú principal
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        // Solo procesar input de pausa si estamos en una escena permitida
        string currentScene = SceneManager.GetActiveScene().name;
        if (!IsSceneAllowed(currentScene))
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
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
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}