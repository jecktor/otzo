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
    }

    void Update()
    {
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
        if (!systemsInitialized)
        {
            TryInitializeSystems();
            if (!systemsInitialized)
            {
                return;
            }
        }

        if (gameClock != null && sleepSystem != null)
        {
            sleepSystem.Sleep();

            //if (DayNightTransitionManager.Instance != null)
            //{
            //    DayNightTransitionManager.Instance.StartSleepTransition();
            //}
            //else
            //{
            gameClock.SleepAndAdvanceTime();
            //}
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
        if (isNearBed)
        {
            float boxWidth = 350;
            float boxHeight = 50;
            float x = (Screen.width - boxWidth) / 2;
            float y = 100;

            GUI.Box(new Rect(x, y, boxWidth, boxHeight), "Presiona la letra E para dormir", guiStyle);

            if (sleepSystem != null)
            {
            }
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