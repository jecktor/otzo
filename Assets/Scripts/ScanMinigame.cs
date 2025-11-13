using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using EasyPeasyFirstPersonController;

public class ScanMiniGame : MonoBehaviour
{
    [Header("Gameplay settings")]
    public KeyCode scanKey = KeyCode.E;
    public float baseWindow = 1.2f;         // start easy (1.2 sec to hit)
    public float minWindow = 0.4f;          // hardest timing window
    public float windowDecayRate = 0.02f;   // how much to shrink window per scan
    public float delayBetweenItems = 0.4f;
    public float diffMult = 0.3f;
    public int totalScans = 0;

    private float currentWindow;
    private int successfulScans;

    // Referencias privadas con auto-búsqueda
    private SkillCheckUI _skillCheckUI;
    private CustomerSpawner _customerSpawner;
    private FirstPersonController _player;
    private AudioSource _sale, _beep, _wrong;

    private float hardScanChance = 0.5f;    // starts at 10%
    private float hardScanChanceIncrease = 0.05f; // +5% each normal scan

    // Properties que auto-buscan las referencias cuando se necesitan
    public SkillCheckUI skillCheckUI
    {
        get
        {
            if (_skillCheckUI == null)
            {
                _skillCheckUI = FindObjectOfType<SkillCheckUI>();
                if (_skillCheckUI == null)
                    Debug.LogWarning("[ScanMiniGame] SkillCheckUI no encontrado en la escena");
            }
            return _skillCheckUI;
        }
    }

    public CustomerSpawner cs
    {
        get
        {
            if (_customerSpawner == null)
            {
                _customerSpawner = FindObjectOfType<CustomerSpawner>();
                if (_customerSpawner == null)
                    Debug.LogWarning("[ScanMiniGame] CustomerSpawner no encontrado en la escena");
            }
            return _customerSpawner;
        }
    }

    public FirstPersonController player
    {
        get
        {
            if (_player == null)
                FindCorrectPlayer();
            return _player;
        }
    }

    // Properties para audio con auto-inicialización
    private AudioSource saleAudio
    {
        get
        {
            if (_sale == null) SetupAudio();
            return _sale;
        }
    }

    private AudioSource beepAudio
    {
        get
        {
            if (_beep == null) SetupAudio();
            return _beep;
        }
    }

    private AudioSource wrongAudio
    {
        get
        {
            if (_wrong == null) SetupAudio();
            return _wrong;
        }
    }

    void Awake()
    {
        currentWindow = baseWindow;
        // Pre-buscar referencias para mejor performance
        FindCorrectPlayer();
    }

    void FindCorrectPlayer()
    {
        FirstPersonController[] allPlayers = FindObjectsOfType<FirstPersonController>();

        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("[ScanMiniGame] No se encontró ningún FirstPersonController en la escena");
            _player = null;
            return;
        }

        // Prioridad 1: Buscar player en la escena actual (no persistente)
        foreach (FirstPersonController p in allPlayers)
        {
            if (p.gameObject.scene.name == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                _player = p;
                Debug.Log($"[ScanMiniGame] Player encontrado en escena actual: {_player.gameObject.name}");
                return;
            }
        }

        // Prioridad 2: Si no hay en escena actual, usar el primero disponible
        _player = allPlayers[0];
        Debug.Log($"[ScanMiniGame] Usando player disponible: {_player.gameObject.name} (escena: {_player.gameObject.scene.name})");
    }

    private void SetupAudio()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length >= 3)
        {
            _sale = audioSources[0];
            _beep = audioSources[1];
            _wrong = audioSources[2];
            Debug.Log("[ScanMiniGame] AudioSources configurados");
        }
        else
        {
            Debug.LogWarning($"[ScanMiniGame] Se necesitan 3 AudioSources, pero se encontraron {audioSources.Length}");
        }
    }

    public void StepDownDifficulty()
    {
        diffMult -= 0.2f;
    }

    public void Stop()
    {
        // Immediately stop any running mini-game logic
        StopAllCoroutines();

        // Hide any visible UI (skill check, feedback text, etc.)
        if (skillCheckUI != null)
            skillCheckUI.Hide();

        Debug.Log("[ScanMiniGame] Force-stopped mini-game.");
    }


    /// <summary>
    /// Runs the scanning mini-game.
    /// </summary>
    public IEnumerator Run(List<GameObject> items, float totalValue, System.Action<float> onComplete)
    {
        // Verificar referencias críticas antes de empezar
        if (skillCheckUI == null || player == null)
        {
            Debug.LogError("[ScanMiniGame] Referencias críticas no encontradas. SkillCheckUI: " +
                          (skillCheckUI != null) + ", Player: " + (player != null));
            onComplete?.Invoke(0f);
            yield break;
        }

        if (cs == null || cs.IsMeltdownInProgress)
        {
            Debug.Log("[ScanMiniGame] CustomerSpawner no disponible o meltdown en progreso");
            onComplete?.Invoke(0f);
            yield break;
        }

        if (items == null || items.Count == 0)
        {
            Debug.Log("[ScanMiniGame] No hay items para escanear");
            onComplete?.Invoke(0f);
            yield break;
        }

        // Desactivar control del player
        player.SetControl(false);

        totalScans++;
        successfulScans = 0;
        float earned = 0f;

        // progressively tighten window
        currentWindow = Mathf.Max(minWindow, baseWindow - totalScans * windowDecayRate);

        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = items[i];
            item.SetActive(true);

            // --- Wait for timing window ---
            float timer = currentWindow;
            // --- Inside Run() loop, for each item ---
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
                    beepAudio.Play();
                }

                // If player failed (pressed outside zone)
                if (!skillCheckUI.GetResult() && Input.GetKeyDown(KeyCode.E))
                {
                    wrongAudio.Play();
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

        // Reactivar control del player
        player.SetControl(true);
        saleAudio.Play();

        Debug.Log($"[ScanMiniGame] Mini-game completado. Precisión: {accuracy:P0}, Pago: ${payout:F2}");
    }

    // Método para forzar la actualización de referencias si es necesario
    public void RefreshReferences()
    {
        _skillCheckUI = null;
        _customerSpawner = null;
        _player = null;
        _sale = null;
        _beep = null;
        _wrong = null;

        FindCorrectPlayer();
        Debug.Log("[ScanMiniGame] Referencias refrescadas");
    }

    // Para debugging
    void OnEnable()
    {
        Debug.Log($"[ScanMiniGame] Activado. Referencias - Player: {_player != null}, SkillCheckUI: {_skillCheckUI != null}, CustomerSpawner: {_customerSpawner != null}");
    }
}