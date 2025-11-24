using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DayNightTransitionManager : MonoBehaviour
{
    public static DayNightTransitionManager Instance;

    [Header("Configuración Transición")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 2f;
    public float imageDisplayDuration = 3f;

    private bool hasShownRoomToStore = false;
    private bool hasShownStoreToHome = false;
    private bool hasShownIntro = false;

    public bool IsTransitioning { get; private set; } = false;
    private float fadeAlpha = 0f;
    private float imageAlpha = 0f;
    private float transitionTimer = 0f;
    private Texture2D blackTexture;
    private Texture2D whiteTexture; // ⚠️ NUEVO: Cachear textura blanca
    private bool imageShown = false;
    private bool sceneChanged = false;
    private Texture2D currentImage;
    private bool isTransitioningToStore = false;
    private float dailyEarnings = 0f;
    private float originalAudioVolume;
    private bool forceBlackScreen = false;
    private string targetSceneName = "";
    private bool isIntroTransition = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTransitionManager();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeTransitionManager()
    {
        CreateBlackTexture();
        CreateWhiteTexture(); // ⚠️ NUEVO: Crear textura blanca una sola vez
        LoadImagesFromResources();
    }

    void LoadImagesFromResources()
    {
        // Precargar las imágenes para evitar delays
        Texture2D loadedImage1 = Resources.Load<Texture2D>("Images/1");
        Texture2D loadedImage2 = Resources.Load<Texture2D>("Images/2");
        Texture2D loadedIntro = Resources.Load<Texture2D>("Images/intro");

        if (loadedImage1 == null)
            Debug.LogWarning("⚠️ No se encontró Images/1");
        if (loadedImage2 == null)
            Debug.LogWarning("⚠️ No se encontró Images/2");
        if (loadedIntro == null)
            Debug.LogWarning("⚠️ No se encontró Images/intro");
    }

    void CreateBlackTexture()
    {
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    // ⚠️ NUEVO: Crear textura blanca una sola vez
    void CreateWhiteTexture()
    {
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (forceBlackScreen)
        {
            StartCoroutine(RemoveBlackScreenAfterFrames());
        }
    }

    IEnumerator RemoveBlackScreenAfterFrames()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        forceBlackScreen = false;
        ResumeGameAndAudio();
        EnableGameUI();
        EnablePauseFunctionality();
        EndTransition();
    }

    public void StartTransitionWithIntroImage(string sceneToLoad)
    {
        if (IsTransitioning) return;

        IsTransitioning = true;
        isTransitioningToStore = false;
        isIntroTransition = true;
        targetSceneName = sceneToLoad;
        fadeAlpha = 0f;
        imageAlpha = 0f;
        transitionTimer = 0f;
        imageShown = false;
        sceneChanged = false;
        forceBlackScreen = false;

        currentImage = Resources.Load<Texture2D>("Images/intro");
        if (currentImage == null)
        {
            Debug.LogWarning("⚠️ Imagen intro no encontrada, cargando escena directamente");
            SceneManager.LoadScene(sceneToLoad);
            IsTransitioning = false;
            return;
        }

        PauseGameAndAudio();
        DisableGameUI();
        DisablePauseFunctionality();

        StartCoroutine(ForceFirstUpdate());
    }

    public void StartTransitionToStore(float sleepQuality)
    {
        if (IsTransitioning) return;

        IsTransitioning = true;
        isTransitioningToStore = true;
        isIntroTransition = false;
        fadeAlpha = 0f;
        imageAlpha = 0f;
        transitionTimer = 0f;
        imageShown = false;
        sceneChanged = false;
        forceBlackScreen = false;

        bool shouldShowImage = !hasShownRoomToStore;
        hasShownRoomToStore = true;

        PrepareStoreTransitionImage(shouldShowImage);

        PauseGameAndAudio();
        DisableGameUI();
        DisablePauseFunctionality();

        StartCoroutine(ForceFirstUpdate());
    }

    public void StartTransitionToHome(float earnings)
    {
        if (IsTransitioning) return;

        IsTransitioning = true;
        isTransitioningToStore = false;
        isIntroTransition = false;
        dailyEarnings = earnings;
        fadeAlpha = 0f;
        imageAlpha = 0f;
        transitionTimer = 0f;
        imageShown = false;
        sceneChanged = false;
        forceBlackScreen = false;

        bool shouldShowImage = !hasShownStoreToHome;
        hasShownStoreToHome = true;

        PrepareHomeTransitionImage(shouldShowImage);

        PauseGameAndAudio();
        DisableGameUI();
        DisablePauseFunctionality();

        StartCoroutine(ForceFirstUpdate());
    }

    IEnumerator ForceFirstUpdate()
    {
        yield return null;
    }

    void PrepareStoreTransitionImage(bool shouldShowImage)
    {
        if (shouldShowImage)
        {
            currentImage = Resources.Load<Texture2D>("Images/1");
            if (currentImage == null)
            {
                Debug.LogWarning("⚠️ No se encontró Images/1");
            }
        }
        else
        {
            currentImage = null;
        }
    }

    void PrepareHomeTransitionImage(bool shouldShowImage)
    {
        if (shouldShowImage)
        {
            currentImage = Resources.Load<Texture2D>("Images/2");
            if (currentImage == null)
            {
                Debug.LogWarning("⚠️ No se encontró Images/2");
            }
        }
        else
        {
            currentImage = null;
        }
    }

    void Update()
    {
        if (!IsTransitioning) return;

        transitionTimer += Time.unscaledDeltaTime;

        if (currentImage != null)
        {
            HandleTransitionWithImage();
        }
        else
        {
            HandleTransitionWithoutImage();
        }
    }

    void HandleTransitionWithImage()
    {
        float totalDuration = fadeInDuration + imageDisplayDuration + fadeOutDuration;
        float progress = transitionTimer / totalDuration;

        if (transitionTimer <= fadeInDuration)
        {
            float t = transitionTimer / fadeInDuration;
            fadeAlpha = SmoothStep(t);

            if (transitionTimer > fadeInDuration * 0.3f)
            {
                float imageT = (transitionTimer - fadeInDuration * 0.3f) / (fadeInDuration * 0.7f);
                imageAlpha = SmoothStep(imageT);
            }
        }
        else if (transitionTimer <= fadeInDuration + imageDisplayDuration)
        {
            fadeAlpha = 1f;
            imageAlpha = 1f;

            if (!imageShown)
            {
                imageShown = true;
            }
        }
        else if (transitionTimer <= totalDuration)
        {
            float t = (transitionTimer - (fadeInDuration + imageDisplayDuration)) / fadeOutDuration;
            fadeAlpha = 1f - SmoothStep(t);
            imageAlpha = 1f - SmoothStep(t);

            if (!sceneChanged && transitionTimer >= fadeInDuration + imageDisplayDuration + (fadeOutDuration * 0.2f))
            {
                ChangeScene();
            }
        }
        else
        {
            EndTransition();
        }
    }

    void HandleTransitionWithoutImage()
    {
        if (transitionTimer <= fadeInDuration * 0.7f)
        {
            float t = transitionTimer / (fadeInDuration * 0.7f);
            fadeAlpha = SmoothStep(t);
        }
        else if (transitionTimer <= fadeInDuration * 0.7f + 0.1f)
        {
            fadeAlpha = 1f;

            if (!sceneChanged)
            {
                ChangeScene();
            }
        }
        else if (transitionTimer <= fadeInDuration * 0.7f + 0.1f + fadeOutDuration * 0.7f)
        {
            float t = (transitionTimer - (fadeInDuration * 0.7f + 0.1f)) / (fadeOutDuration * 0.7f);
            fadeAlpha = 1f - SmoothStep(t);
        }
        else
        {
            EndTransition();
        }
    }

    float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    void ChangeScene()
    {
        if (sceneChanged) return;

        sceneChanged = true;
        forceBlackScreen = true;

        string targetScene = isIntroTransition ? targetSceneName : (isTransitioningToStore ? "SampleScene" : "room");

        fadeAlpha = 1f;
        imageAlpha = 0f;

        Debug.Log($"🔄 Cambiando a escena: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    void EndTransition()
    {
        IsTransitioning = false;
        fadeAlpha = 0f;
        imageAlpha = 0f;
        imageShown = false;
        sceneChanged = false;
        forceBlackScreen = false;
        isIntroTransition = false;
        targetSceneName = "";

        if (!isTransitioningToStore)
        {
            dailyEarnings = 0f;
        }

        Debug.Log("✅ Transición completada");
    }

    void PauseGameAndAudio()
    {
        Time.timeScale = 0f;
        originalAudioVolume = AudioListener.volume;
        AudioListener.volume = 0f;
    }

    void ResumeGameAndAudio()
    {
        Time.timeScale = 1f;
        AudioListener.volume = originalAudioVolume;
    }

    void DisableGameUI()
    {
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.gameClock != null)
        {
            GameManagerPersistent.Instance.gameClock.enabled = false;
        }

        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            GameManagerPersistent.Instance.sleepSystem.enabled = false;
        }
    }

    void EnableGameUI()
    {
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.gameClock != null)
        {
            GameManagerPersistent.Instance.gameClock.enabled = true;
        }

        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            GameManagerPersistent.Instance.sleepSystem.enabled = true;
        }
    }

    void DisablePauseFunctionality()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.enabled = false;
        }
    }

    void EnablePauseFunctionality()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.enabled = true;
        }
    }

    void OnGUI()
    {
        if (!IsTransitioning && !forceBlackScreen) return;

        if (forceBlackScreen)
        {
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            return;
        }

        if (fadeAlpha > 0f)
        {
            GUI.color = new Color(1, 1, 1, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        }

        if (currentImage != null && imageAlpha > 0f)
        {
            GUI.color = new Color(1, 1, 1, imageAlpha);
            Rect fullScreenRect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.DrawTexture(fullScreenRect, currentImage, ScaleMode.StretchToFill);
        }

        if (currentImage != null && imageAlpha >= 0.8f)
        {
            DrawLoadingBar();
        }
    }

    void DrawLoadingBar()
    {
        float totalDuration = fadeInDuration + imageDisplayDuration + fadeOutDuration;
        float progress = Mathf.Clamp01(transitionTimer / totalDuration);

        float barWidth = Screen.width * 0.4f;
        float barHeight = 12f;
        float barX = (Screen.width - barWidth) * 0.5f;
        float barY = Screen.height - 60f;

        Rect progressRect = new Rect(barX, barY, barWidth * progress, barHeight);
        Color progressColor = Color.Lerp(new Color(1f, 0.5f, 0.2f), new Color(0.3f, 0.9f, 0.4f), progress);
        GUI.color = progressColor;

        // ⚠️ CORRECCIÓN: Usar textura cacheada en lugar de Texture2D.whiteTexture
        GUI.DrawTexture(progressRect, whiteTexture);
    }

    [ContextMenu("Reset Transition Flags")]
    public void ResetTransitionFlags()
    {
        hasShownRoomToStore = false;
        hasShownStoreToHome = false;
        hasShownIntro = false;
        Debug.Log("🔄 Flags de transición reseteadas");
    }

    void OnDestroy()
    {
        // ⚠️ CRÍTICO: Limpiar texturas al destruir
        if (blackTexture != null)
        {
            Destroy(blackTexture);
            blackTexture = null;
        }

        if (whiteTexture != null)
        {
            Destroy(whiteTexture);
            whiteTexture = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;

        Debug.Log("🧹 DayNightTransitionManager limpiado");
    }
}