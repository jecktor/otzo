using UnityEngine;

public class BedTriggerSleep : MonoBehaviour
{
    [Header("Configuración Cama")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 2f;

    private Transform player;
    private bool isNearBed = false;
    private GameClock gameClock;
    private SleepSystem sleepSystem;
    private GUIStyle guiStyle;
    private Texture2D backgroundTex;
    private bool systemsInitialized = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        CreateGUIStyle();

        TryInitializeSystems();
        EnsureGameDataManagerExists();
        EnsureEmailManagerExists();
    }

    void EnsureGameDataManagerExists()
    {
        if (GameDataManager.Instance == null)
        {
            GameObject gameDataManagerObj = new GameObject("GameDataManager");
            gameDataManagerObj.AddComponent<GameDataManager>();
        }
    }

    void EnsureEmailManagerExists()
    {
        if (EmailManager.Instance == null)
        {
            GameObject emailManagerObj = new GameObject("EmailManager");
            emailManagerObj.AddComponent<EmailManager>();
        }
    }

    void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.CurrentState == UIManager.GameState.Paused)
            return;

        if (player == null) return;

        if (!systemsInitialized)
        {
            TryInitializeSystems();
        }

        CheckBedProximity();

        if (isNearBed && Input.GetKeyDown(interactionKey))
        {
            SleepInBed();
        }
    }

    void TryInitializeSystems()
    {
        if (GameManagerPersistent.Instance != null)
        {
            gameClock = GameManagerPersistent.Instance.gameClock;
            sleepSystem = GameManagerPersistent.Instance.sleepSystem;

            if (gameClock != null && sleepSystem != null)
            {
                systemsInitialized = true;
                Debug.Log("✅ Sistemas de cama inicializados correctamente");
            }
        }
    }

    void CheckBedProximity()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        isNearBed = distance <= interactionRange;
    }

    void SleepInBed()
    {
        if (!EmailManager.Instance.HasReadFirstDayEmail)
        {
            Debug.Log("📧 Debes leer el correo primero antes de dormir");
            return;
        }

        if (!systemsInitialized)
        {
            TryInitializeSystems();
            if (!systemsInitialized)
            {
                Debug.LogError("❌ Sistemas no inicializados");
                return;
            }
        }

        if (gameClock != null && sleepSystem != null)
        {
            int dayBeforeSleep = gameClock.CurrentDay;

            // ⚠️ YA NO NECESITAS ESTO - GameClock lo maneja automáticamente
            // gameClock.CompleteFirstDay(); ← ELIMINAR ESTA LÍNEA

            float sleepBefore = sleepSystem.CurrentSleepQuality;
            sleepSystem.Sleep();
            float sleepAfter = sleepSystem.CurrentSleepQuality;

            gameClock.SleepAndAdvanceTime();

            Debug.Log($"🛌 Dormido - Día: {dayBeforeSleep} → {gameClock.CurrentDay}, Sueño: {sleepBefore:F1}% → {sleepAfter:F1}%");
        }
    }
    void CreateGUIStyle()
    {
        guiStyle = new GUIStyle();
        guiStyle.normal.textColor = Color.white;
        guiStyle.fontSize = 20;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.fontStyle = FontStyle.Bold;

        backgroundTex = new Texture2D(1, 1);
        backgroundTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        backgroundTex.Apply();
        guiStyle.normal.background = backgroundTex;
    }

    void OnGUI()
    {
        if (UIManager.Instance != null && UIManager.Instance.CurrentState == UIManager.GameState.Paused)
            return;

        if (isNearBed)
        {
            float boxWidth = 350;
            float boxHeight = 50;
            float x = (Screen.width - boxWidth) / 2;
            float y = 100;

            string message = EmailManager.Instance.HasReadFirstDayEmail ?
                "Presiona la letra E para dormir" :
                "Revisa tu correo en la computadora";

            GUI.Box(new Rect(x, y, boxWidth, boxHeight), message, guiStyle);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    void OnDestroy()
    {
        if (backgroundTex != null)
            Destroy(backgroundTex);
    }
}