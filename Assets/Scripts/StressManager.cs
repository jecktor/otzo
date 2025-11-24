using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class CustomerStressManager : MonoBehaviour
{
    [Header("Settings")]
    public float maxStress = 10f;              // time (seconds) to reach max stress
    public CustomerSpawner cs;
    public Slider stressBar;                   // UI slider (0–1)
    public QueueManager qm;
    public ScanMiniGame smg;
    public TextMeshProUGUI mt;

    [Header("Visuals")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    public string[] mentadas;

    private float stressLevel = 0f;
    private bool meltdownTriggered = false;
    private bool timerPlaying;

    private Image fillImage;

    public AudioSource timer;
    public AudioSource fail;

    void Start()
    {
        // Inicializar GameEvents si no existe
        if (GameEvents.Instance == null)
        {
            new GameObject("GameEvents").AddComponent<GameEvents>();
        }

        if (stressBar != null)
        {
            stressBar.minValue = 0f;
            stressBar.maxValue = 1f;
            fillImage = stressBar.fillRect.GetComponent<Image>();
        }

        // Validar referencias críticas
        if (fail == null) Debug.LogError("Fail AudioSource not assigned in CustomerStressManager");
        if (mt == null) Debug.LogError("TextMeshProUGUI not assigned in CustomerStressManager");
        if (smg == null) Debug.LogError("ScanMiniGame not assigned in CustomerStressManager");
        if (cs == null) Debug.LogError("CustomerSpawner not assigned in CustomerStressManager");
        if (qm == null) Debug.LogError("QueueManager not assigned in CustomerStressManager");
    }

    void Update()
    {
        if (cs == null || qm == null || smg == null || meltdownTriggered) return;

        if (qm.IsQueueFull && cs.IsStoreFull)
        {
            if (!timerPlaying && timer != null)
            {
                timer.Play();
                timerPlaying = true;
            }

            stressLevel += Time.deltaTime;
            if (stressLevel >= maxStress)
            {
                meltdownTriggered = true;
                StartCoroutine(TriggerMeltdown());
            }
        }
        else
        {
            if (timerPlaying && timer != null)
            {
                timer.Stop();
                timerPlaying = false;
            }
            // relieve stress when queue is under control
            stressLevel = Mathf.Max(0f, stressLevel - Time.deltaTime * 1.5f);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (stressBar == null) return;
        float ratio = stressLevel / maxStress;
        stressBar.value = ratio;

        if (fillImage != null)
        {
            if (ratio < 0.5f) fillImage.color = normalColor;
            else if (ratio < 0.8f) fillImage.color = warningColor;
            else fillImage.color = dangerColor;
        }
    }

    IEnumerator TriggerMeltdown()
    {
        Debug.Log("Customer meltdown! Everyone leaves!");

        // Usar eventos en lugar de referencia directa al player
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.SetPlayerControl(true);
            GameEvents.Instance.TriggerCustomerMeltdown();
        }

        // Reproducir sonido de fallo
        if (fail != null)
            fail.Play();

        stressLevel = maxStress;
        UpdateUI();

        // Mostrar mensaje aleatorio
        if (mt != null && mentadas != null && mentadas.Length > 0)
            mt.text = mentadas[Random.Range(0, mentadas.Length)];

        // Detener mini juego y limpiar clientes
        if (smg != null)
        {
            smg.Stop();
            smg.StepDownDifficulty();
        }

        if (cs != null)
            cs.ClearAllCustomers();

        // Esperar el tiempo de penalización
        float penaltyTime = cs != null ? cs.meltdownPenalty : 3f;
        yield return new WaitForSeconds(penaltyTime);

        // Resetear el estado
        meltdownTriggered = false;
        stressLevel = 0f;
        UpdateUI();

        if (mt != null)
            mt.text = "";
    }

    public void ResetStress()
    {
        stressLevel = 0f;
        meltdownTriggered = false;
        UpdateUI();
    }
}