using UnityEngine;
using System.Collections;

public class DayNightTransitionManager : MonoBehaviour
{
    public static DayNightTransitionManager Instance;

    [Header("Configuración Transición")]
    public float fadeDuration = 2f;
    public float blackScreenDuration = 1f;
    public float dialogDuration = 10f;

    [Header("Mensajes Fin de Día")]
    public string[] goodDayMessages = {
        "¡Hoy fue un gran día! Las ventas fueron excelentes.",
        "Los clientes estaban muy contentos con el servicio. Buen trabajo.",
        "Día productivo, aprendí mucho sobre el negocio.",
        "Todo salió según lo planeado. Mañana será aún mejor."
    };
    public string[] averageDayMessages = {
        "Día normal, nada extraordinario pero tampoco malo.",
        "Las ventas fueron regulares. Mañana puede ser mejor.",
        "Un día más de trabajo, sin mayores complicaciones.",
        "Ni bueno ni malo, solo otro día en la tienda."
    };
    public string[] badDayMessages = {
        "Hoy fue complicado, los clientes estaban difíciles.",
        "Las ventas no fueron buenas. Espero que mañana sea mejor.",
        "Día agotador, muchos problemas que resolver.",
        "No fue mi mejor día. Necesito descansar y recuperarme."
    };

    public bool IsTransitioning { get; private set; } = false;
    private float fadeAlpha = 0f;
    private float transitionTimer = 0f;
    private Texture2D blackTexture;
    private bool dialogShown = false;
    private bool sceneChanged = false;
    private string currentDialog = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad aquí - será manejado por el sistema persistente
            InitializeTransitionManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeTransitionManager()
    {
        CreateBlackTexture();
        EnsureDialogSystemExists();
    }

    void CreateBlackTexture()
    {
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    void EnsureDialogSystemExists()
    {
        DialogSystem existingDialogSystem = FindFirstObjectByType<DialogSystem>();
        if (existingDialogSystem == null)
        {
            GameObject dialogSystemObj = new GameObject("DialogSystem");
            dialogSystemObj.AddComponent<DialogSystem>();
        }
    }

    public void StartDayEndTransition()
    {
        if (IsTransitioning) return;

        IsTransitioning = true;
        fadeAlpha = 0f;
        transitionTimer = 0f;
        dialogShown = false;
        sceneChanged = false;

        PrepareDialog();
    }

    void PrepareDialog()
    {
        string[] selectedMessages = GetDayEndMessages();
        int randomIndex = Random.Range(0, selectedMessages.Length);
        currentDialog = selectedMessages[randomIndex];
    }

    void Update()
    {
        if (!IsTransitioning) return;

        transitionTimer += Time.deltaTime;

        if (transitionTimer <= fadeDuration)
        {
            fadeAlpha = Mathf.Clamp01(transitionTimer / fadeDuration);
        }
        else if (transitionTimer <= fadeDuration + blackScreenDuration)
        {
            fadeAlpha = 1f;

            if (!dialogShown && transitionTimer >= fadeDuration + 0.1f)
            {
                ShowDialog();
            }
        }
        else if (transitionTimer <= fadeDuration + blackScreenDuration + dialogDuration)
        {
            fadeAlpha = 1f;

            if (!sceneChanged && transitionTimer >= fadeDuration + blackScreenDuration + 1f)
            {
                ChangeToNightScene();
            }
        }
        else if (transitionTimer <= fadeDuration + blackScreenDuration + dialogDuration + fadeDuration)
        {
            float fadeOutTime = transitionTimer - (fadeDuration + blackScreenDuration + dialogDuration);
            fadeAlpha = 1f - Mathf.Clamp01(fadeOutTime / fadeDuration);
        }
        else
        {
            EndTransition();
        }
    }

    void ShowDialog()
    {
        dialogShown = true;
        StartCoroutine(ShowDialogAfterFrame());
    }

    IEnumerator ShowDialogAfterFrame()
    {
        yield return null;

        DialogSystem dialogSystem = FindFirstObjectByType<DialogSystem>();

        if (dialogSystem != null)
        {
            dialogSystem.ShowInstantDialog(currentDialog, dialogDuration);
        }
        else
        {
            Debug.LogWarning("DialogSystem no encontrado, usando fallback GUI");
            StartCoroutine(ShowDialogFallback());
        }
    }

    IEnumerator ShowDialogFallback()
    {
        float fallbackTimer = 0f;
        while (fallbackTimer < 3f)
        {
            fallbackTimer += Time.deltaTime;
            yield return null;
        }
    }

    void ChangeToNightScene()
    {
        if (sceneChanged) return;

        sceneChanged = true;

        // CAMBIO AQUÍ: Acceder a través de GameManagerPersistent
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.gameClock != null)
        {
            GameManagerPersistent.Instance.gameClock.ChangeToNightScene();
        }
        else
        {
            Debug.LogError("No se puede cambiar a escena nocturna: GameClock no disponible");

            // Fallback: cambiar escena directamente
            string nightScene = "room";
            if (IsSceneInBuildSettings(nightScene))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nightScene);
            }
        }
    }

    // Método auxiliar para verificar escenas en build settings
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (buildSceneName == sceneName)
                return true;
        }
        return false;
    }

    string[] GetDayEndMessages()
    {
        int randomChoice = Random.Range(0, 3);
        switch (randomChoice)
        {
            case 0: return goodDayMessages;
            case 1: return averageDayMessages;
            default: return badDayMessages;
        }
    }

    void EndTransition()
    {
        IsTransitioning = false;
        fadeAlpha = 0f;
        dialogShown = false;
        sceneChanged = false;
    }

    void OnGUI()
    {
        if (fadeAlpha > 0f)
        {
            GUI.color = new Color(1, 1, 1, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            GUI.color = Color.white;
        }

        if (dialogShown && fadeAlpha >= 1f && !string.IsNullOrEmpty(currentDialog))
        {
            GUIStyle dialogStyle = new GUIStyle();
            dialogStyle.normal.textColor = Color.white;
            dialogStyle.fontSize = 24;
            dialogStyle.alignment = TextAnchor.MiddleCenter;
            dialogStyle.wordWrap = true;
            dialogStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));

            Rect dialogRect = new Rect(Screen.width * 0.1f, Screen.height * 0.7f,
                                     Screen.width * 0.8f, Screen.height * 0.2f);

            GUI.Label(dialogRect, currentDialog, dialogStyle);
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}