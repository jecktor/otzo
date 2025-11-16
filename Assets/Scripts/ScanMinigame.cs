using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using EasyPeasyFirstPersonController;

public class ScanMiniGame : MonoBehaviour
{
    [Header("Gameplay settings")]
    public KeyCode scanKey = KeyCode.E;
    public float baseWindow = 1.2f;
    public float minWindow = 0.4f;
    public float windowDecayRate = 0.02f;
    public float delayBetweenItems = 0.4f;
    public float diffMult = 0.3f;
    public int totalScans = 0;

    private float currentWindow;
    private int successfulScans;

    public SkillCheckUI skillCheckUI;
    public CustomerSpawner cs;
    public FirstPersonController player;

    public AudioSource sale;
    public AudioSource beep;
    public AudioSource wrong;

    private float hardScanChance = 0.5f;
    private float hardScanChanceIncrease = 0.05f;

    void Awake()
    {
        currentWindow = baseWindow;
    }

    void Start()
    {
        // Buscar referencias en Start() en lugar de Awake() para mejor timing
        FindMissingReferences();
    }

    void OnEnable()
    {
        // También buscar cuando el objeto se active (al volver a la escena)
        StartCoroutine(DelayedReferenceSearch());
    }

    System.Collections.IEnumerator DelayedReferenceSearch()
    {
        // Esperar un frame para que todo esté inicializado
        yield return null;
        FindMissingReferences();
    }

    void FindMissingReferences()
    {
        bool foundNewReference = false;

        if (skillCheckUI == null)
        {
            skillCheckUI = FindObjectOfType<SkillCheckUI>();
            if (skillCheckUI != null)
            {
                Debug.Log($"[ScanMiniGame] SkillCheckUI encontrado: {skillCheckUI.gameObject.name}");
                foundNewReference = true;
            }
        }

        if (cs == null)
        {
            cs = FindObjectOfType<CustomerSpawner>();
            if (cs != null)
            {
                Debug.Log($"[ScanMiniGame] CustomerSpawner encontrado: {cs.gameObject.name}");
                foundNewReference = true;
            }
        }

        if (player == null)
        {
            player = FindCorrectPlayer();
            if (player != null)
            {
                Debug.Log($"[ScanMiniGame] Player encontrado: {player.gameObject.name}");
                foundNewReference = true;
            }
        }

        // Buscar AudioSources si faltan
        if (sale == null || beep == null || wrong == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length >= 3)
            {
                sale = audioSources[0];
                beep = audioSources[1];
                wrong = audioSources[2];
                Debug.Log("[ScanMiniGame] AudioSources configurados");
                foundNewReference = true;
            }
        }

        if (foundNewReference)
        {
            Debug.Log($"[ScanMiniGame] Referencias actualizadas - Player: {player != null}, SkillCheckUI: {skillCheckUI != null}");
        }
    }

    FirstPersonController FindCorrectPlayer()
    {
        FirstPersonController[] allPlayers = FindObjectsOfType<FirstPersonController>(true); // Incluir inactivos

        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("[ScanMiniGame] No se encontró ningún FirstPersonController");
            return null;
        }

        // Prioridad 1: Buscar el player que esté activo y habilitado
        foreach (FirstPersonController p in allPlayers)
        {
            if (p != null && p.gameObject.activeInHierarchy && p.enabled)
            {
                return p;
            }
        }

        // Prioridad 2: Buscar cualquier player disponible
        foreach (FirstPersonController p in allPlayers)
        {
            if (p != null)
            {
                return p;
            }
        }

        return allPlayers.Length > 0 ? allPlayers[0] : null;
    }

    public void StepDownDifficulty()
    {
        diffMult -= 0.2f;
    }

    public void Stop()
    {
        StopAllCoroutines();

        // Re-habilitar el control del player inmediatamente
        if (player != null)
        {
            player.SetControl(true);
            Debug.Log("[ScanMiniGame] Control del player re-habilitado");
        }

        if (skillCheckUI != null)
            skillCheckUI.Hide();

        Debug.Log("[ScanMiniGame] Force-stopped mini-game.");
    }

    /// <summary>
    /// Runs the scanning mini-game.
    /// </summary>
    public IEnumerator Run(List<GameObject> items, float totalValue, System.Action<float> onComplete)
    {
        // Verificar y actualizar referencias antes de empezar
        FindMissingReferences();

        // Verificar referencias críticas antes de empezar
        if (skillCheckUI == null || player == null)
        {
            Debug.LogError($"[ScanMiniGame] Referencias faltantes - SkillCheckUI: {skillCheckUI != null}, Player: {player != null}");

            // Intentar una búsqueda de emergencia
            RefreshReferences();

            if (skillCheckUI == null || player == null)
            {
                Debug.LogError($"[ScanMiniGame] ERROR CRÍTICO - No se pueden encontrar referencias después de búsqueda de emergencia");
                onComplete?.Invoke(0f);
                yield break;
            }
        }

        if (cs == null || cs.IsMeltdownInProgress)
        {
            onComplete?.Invoke(0f);
            yield break;
        }

        if (items == null || items.Count == 0)
        {
            onComplete?.Invoke(0f);
            yield break;
        }

        player.SetControl(false);

        totalScans++;
        successfulScans = 0;
        float earned = 0f;

        currentWindow = Mathf.Max(minWindow, baseWindow - totalScans * windowDecayRate);

        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = items[i];
            item.SetActive(true);

            bool scanned = false;
            bool isHardScan = Random.value < hardScanChance;

            if (isHardScan)
            {
                Debug.Log("Hard scan");
                skillCheckUI.Show(diffMult + 0.3f);
                hardScanChance = 0.1f;
            }
            else
            {
                skillCheckUI.Show(diffMult);
                hardScanChance = Mathf.Min(1f, hardScanChance + hardScanChanceIncrease);
            }

            int attempts = 1;

            while (!scanned)
            {
                yield return null;

                if (skillCheckUI.GetResult())
                {
                    scanned = true;
                    successfulScans++;
                    earned += totalValue / items.Count;
                    skillCheckUI.Hide();
                    if (beep != null) beep.Play();
                }

                if (!skillCheckUI.GetResult() && Input.GetKeyDown(KeyCode.E))
                {
                    if (wrong != null) wrong.Play();
                    skillCheckUI.Stop();
                    yield return new WaitForSeconds(2f);
                    if (isHardScan)
                    {
                        Debug.Log("No escape");
                        skillCheckUI.Show((diffMult + 0.3f) - (0.02f * attempts));
                        hardScanChance = 0.1f;
                    }
                    else
                    {
                        skillCheckUI.Show(diffMult);
                        hardScanChance = Mathf.Min(1f, hardScanChance + hardScanChanceIncrease);
                    }

                    attempts++;
                }
            }

            diffMult = Mathf.Clamp(0.3f + totalScans * 0.03f, 0.3f, 1.3f);

            item.SetActive(false);
            yield return new WaitForSeconds(delayBetweenItems);
        }

        float accuracy = (float)successfulScans / items.Count;
        float payout = totalValue * accuracy;

        onComplete?.Invoke(payout);

        player.SetControl(true);
        if (sale != null) sale.Play();
    }

    // Método para forzar la actualización de referencias
    public void RefreshReferences()
    {
        Debug.Log("[ScanMiniGame] Refrescando referencias...");

        skillCheckUI = null;
        cs = null;
        player = null;

        FindMissingReferences();
    }
}