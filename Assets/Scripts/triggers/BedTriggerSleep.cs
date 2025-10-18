using UnityEngine;

public class BedTriggerSleep : MonoBehaviour
{
    [Header("Configuración Cama")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 2f;

    private Transform player;
    private bool isNearBed = false;
    private GameClock gameClock;
    private GUIStyle guiStyle;
    private Texture2D backgroundTex;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gameClock = GameClock.Instance;
        CreateGUIStyle();
    }

    void Update()
    {
        if (player == null) return;

        CheckBedProximity();

        if (isNearBed && Input.GetKeyDown(interactionKey))
        {
            SleepInBed();
        }
    }

    void CheckBedProximity()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        isNearBed = distance <= interactionRange;
    }

    void SleepInBed()
    {
        if (gameClock != null)
        {
            gameClock.SleepAndAdvanceTime();
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