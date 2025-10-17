using UnityEngine;
using UnityEngine.SceneManagement;

public class BedTriggerSleep : MonoBehaviour
{
    [Header("Configuración Cama")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 2f;
    public GameObject interactionUI; // UI que muestra "Presiona E para dormir"

    private Transform player;
    private bool isNearBed = false;
    private GameClock gameClock;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gameClock = GameClock.Instance;

        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        CheckBedProximity();

        if (isNearBed)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                SleepInBed();
            }

            // Mostrar UI de interacción
            if (interactionUI != null)
                interactionUI.SetActive(true);
        }
        else
        {
            // Ocultar UI de interacción
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }

    void CheckBedProximity()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        isNearBed = distance <= interactionRange;
    }

    void SleepInBed()
    {
        Debug.Log("Durmiendo en la cama...");

        if (gameClock != null)
        {
            gameClock.SleepAndAdvanceTime();
        }
        else
        {
            Debug.LogError("GameClock no encontrado!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
