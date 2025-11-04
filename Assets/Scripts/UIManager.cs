using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameObject mainMenuCanvas;
    private GameObject pauseMenuCanvas;
    private GameObject eventSystem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("🟢 UIManager inicializado");

            // Crear EventSystem solo si no existe
            CreateEventSystem();

            CreateMainMenu();
            CreatePauseMenu();

            ShowMainMenu();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateEventSystem()
    {
        // Buscar si ya existe un EventSystem en la escena
        EventSystem existingEventSystem = FindObjectOfType<EventSystem>();

        if (existingEventSystem == null)
        {
            // Crear nuevo EventSystem
            eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystem);
            Debug.Log("✅ EventSystem creado");
        }
        else
        {
            // Si ya existe, usar ese y marcarlo para no destruir
            eventSystem = existingEventSystem.gameObject;
            DontDestroyOnLoad(eventSystem);
            Debug.Log("ℹ️ EventSystem ya existente encontrado");
        }
    }

    void CreateMainMenu()
    {
        Debug.Log("📋 Creando menú principal...");

        // Canvas del menú principal
        mainMenuCanvas = CreateCanvas("MainMenuCanvas", 10);

        // Fondo oscuro
        CreateFullScreenImage(mainMenuCanvas, new Color(0.1f, 0.1f, 0.1f, 1f));

        // Título
        CreateSimpleText(mainMenuCanvas, "TIENDA SIMULATOR", 72,
                        new Vector2(0.5f, 0.75f), new Vector2(800, 100), Color.yellow);

        // Botón JUGAR
        CreateSimpleButton(mainMenuCanvas, "JUGAR", 42,
                          new Vector2(0.5f, 0.5f), new Vector2(400, 80),
                          () => {
                              Debug.Log("🎮 Botón JUGAR clickeado");
                              GameManager.Instance.StartGame();
                          });

        // Botón SALIR
        CreateSimpleButton(mainMenuCanvas, "SALIR", 42,
                          new Vector2(0.5f, 0.35f), new Vector2(400, 80),
                          () => {
                              Debug.Log("👋 Botón SALIR clickeado");
                              GameManager.Instance.QuitGame();
                          });

        Debug.Log("✅ Menú principal creado");
    }

    void CreatePauseMenu()
    {
        Debug.Log("📋 Creando menú de pausa...");

        // Canvas del menú de pausa
        pauseMenuCanvas = CreateCanvas("PauseMenuCanvas", 20);
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
                              GameManager.Instance.ResumeGame();
                          });

        // Botón MENÚ PRINCIPAL
        CreateSimpleButton(pauseMenuCanvas, "MENÚ PRINCIPAL", 36,
                          new Vector2(0.5f, 0.35f), new Vector2(400, 80),
                          () => {
                              Debug.Log("🏠 Botón MENÚ PRINCIPAL clickeado");
                              GameManager.Instance.QuitToMainMenu();
                          });

        Debug.Log("✅ Menú de pausa creado (inicialmente oculto)");
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
        // Botón simple
        GameObject buttonObj = new GameObject("Button_" + text);
        buttonObj.transform.SetParent(parent.transform);

        // RectTransform
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = new Vector2(anchoredPosition.x * 2 - 1, anchoredPosition.y * 2 - 1) * 500f;

        // Imagen del botón
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Componente Button
        Button button = buttonObj.AddComponent<Button>();

        // Colores del botón
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.selectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        // Evento de clic
        button.onClick.AddListener(() => {
            Debug.Log($"🟢 Botón '{text}' recibió clic");
            action?.Invoke();
        });

        // Texto del botón
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

        // Asegurar que el texto esté encima
        textObj.transform.SetAsLastSibling();

        Debug.Log($"🔘 Botón '{text}' creado en posición {anchoredPosition}");
    }

    public void ShowMainMenu()
    {
        Debug.Log("📱 Mostrando menú principal");
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;

        // MOSTRAR CURSOR en menú principal
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("🖱️ Cursor mostrado en menú principal");
    }

    public void ShowPauseMenu()
    {
        Debug.Log("📱 Mostrando menú de pausa");
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
            Canvas.ForceUpdateCanvases();

            // MOSTRAR Y DESBLOQUEAR EL CURSOR
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("🖱️ Cursor mostrado y desbloqueado para menú de pausa");

            // Desactivar PlayerInput cuando el menú de pausa está activo
            if (MainCharacterScript.Instance != null)
            {
                PlayerInput playerInput = MainCharacterScript.Instance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.enabled = false;
                    Debug.Log("🔴 PlayerInput desactivado para menú de pausa");
                }
            }
        }
    }

    public void HideAllMenus()
    {
        Debug.Log("📱 Ocultando todos los menús");
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);

            // OCULTAR Y BLOQUEAR EL CURSOR (solo si estamos en modo juego)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Debug.Log("🖱️ Cursor ocultado y bloqueado para modo juego");
            }

            // Reactivar PlayerInput cuando se cierra el menú
            if (MainCharacterScript.Instance != null)
            {
                PlayerInput playerInput = MainCharacterScript.Instance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.enabled = true;
                    Debug.Log("🟢 PlayerInput reactivado");
                }
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ GameManager.Instance es null");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"⎈ ESC presionado - Estado actual: {GameManager.Instance.CurrentState}");

            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                Debug.Log("⏸️ Cambiando a estado Paused");
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                Debug.Log("▶️ Cambiando a estado Playing");
                GameManager.Instance.ResumeGame();
            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this && eventSystem != null)
        {
            Destroy(eventSystem);
        }
    }
}