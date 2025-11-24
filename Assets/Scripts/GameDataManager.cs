using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    private FirebaseFirestore db;
    private bool isFirebaseInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            db = FirebaseFirestore.DefaultInstance;
            isFirebaseInitialized = true;
            Debug.Log("✅ Firebase inicializado correctamente");
        }
        else
        {
            Debug.LogError($"❌ Firebase error: {dependencyStatus}");
        }
    }

    public async void SaveUserDataToFirebase()
    {
        if (!isFirebaseInitialized || string.IsNullOrEmpty(GlobalUser.Instance?.Username))
        {
            Debug.LogWarning("⚠️ No se puede guardar: Firebase no inicializado o usuario no logeado");
            return;
        }

        try
        {
            float money = PlayerPrefs.GetFloat("PlayerMoney", 2500f);
            float sleepQuality = PlayerPrefs.GetFloat("SleepQuality", 100f);
            int currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
            string upgrades = PlayerPrefs.GetString("PurchasedUpgrades", "");

            // **CORRECCIÓN: Asegurar que el día nunca sea 0**
            if (currentDay <= 0)
            {
                currentDay = 1;
                PlayerPrefs.SetInt("CurrentDay", currentDay);
                PlayerPrefs.Save();
                Debug.LogWarning("🔄 Día corregido de 0 a 1 antes de guardar en Firebase");
            }

            var data = new Dictionary<string, object>
            {
                { "username", GlobalUser.Instance.Username },
                { "money", money },
                { "sleepQuality", sleepQuality },
                { "dayGame", currentDay },
                { "upgrades", upgrades },
            };

            DocumentReference docRef = db.Collection("players").Document(GlobalUser.Instance.Username);
            await docRef.SetAsync(data, SetOptions.MergeAll);

            Debug.Log($"✅ Datos guardados en Firebase - Día: {currentDay}, Dinero: ${money:F2}");

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error guardando en Firebase: {ex.Message}");
        }
    }

    public async void LoadUserDataFromFirebase()
    {
        if (!isFirebaseInitialized || string.IsNullOrEmpty(GlobalUser.Instance?.Username))
        {
            Debug.LogWarning("⚠️ No se puede cargar: Firebase no inicializado o usuario no logeado");
            return;
        }

        try
        {
            DocumentReference docRef = db.Collection("players").Document(GlobalUser.Instance.Username);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                float money = data.ContainsKey("money") ? System.Convert.ToSingle(data["money"]) : 2500f;
                float sleepQuality = data.ContainsKey("sleepQuality") ? System.Convert.ToSingle(data["sleepQuality"]) : 100f;
                int currentDay = data.ContainsKey("dayGame") ? System.Convert.ToInt32(data["dayGame"]) : 1;
                string upgrades = data.ContainsKey("upgrades") ? data["upgrades"].ToString() : "";

                // **CORRECCIÓN: Si el día es 0, cambiarlo a 1**
                if (currentDay <= 0)
                {
                    currentDay = 1;
                    Debug.Log("🔄 Día corregido de 0 a 1 al cargar de Firebase");
                }

                PlayerPrefs.SetFloat("PlayerMoney", money);
                PlayerPrefs.SetFloat("SleepQuality", sleepQuality);
                PlayerPrefs.SetInt("CurrentDay", currentDay);
                PlayerPrefs.SetString("PurchasedUpgrades", upgrades);
                PlayerPrefs.Save();

                UpdateGameVariables(money, sleepQuality, currentDay, upgrades);

                Debug.Log($"✅ Datos cargados desde Firebase - Día: {currentDay}, Dinero: ${money:F2}");
            }
            else
            {
                Debug.Log("📝 No se encontraron datos, creando nuevos");
                CreateDefaultUserData();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error cargando de Firebase: {ex.Message}");
        }
    }

    private void UpdateGameVariables(float money, float sleepQuality, int currentDay, string upgrades)
    {
        // **CORRECCIÓN: Asegurar que el día nunca sea 0**
        if (currentDay <= 0)
        {
            currentDay = 1;
            Debug.LogWarning("🔄 Día corregido de 0 a 1 en UpdateGameVariables");
        }

        PlayerWallet.totalMoney = money;

        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            GameManagerPersistent.Instance.sleepSystem.ModifySleepQuality(sleepQuality - GameManagerPersistent.Instance.sleepSystem.CurrentSleepQuality);
        }

        // **CORRECCIÓN: Actualizar el GameClock si existe**
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.gameClock != null)
        {
            // Solo actualizar la hora si no es el primer día
            if (currentDay > 1)
            {
                GameManagerPersistent.Instance.gameClock.SetExactTime(8, 0);
            }
        }

        if (ShopManager.Instance != null && !string.IsNullOrEmpty(upgrades))
        {
            ShopManager.Instance.ResetPurchases();
            string[] upgradeList = upgrades.Split(';');
            foreach (string upgrade in upgradeList)
            {
                if (!string.IsNullOrEmpty(upgrade))
                {
                    ShopManager.Instance.PurchaseUpgrade(upgrade);
                }
            }
        }

        // **GUARDAR los valores corregidos**
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.Save();
    }

    public void CreateDefaultUserData()
    {
        float defaultMoney = 2500f;
        float defaultSleep = 100f;
        int defaultDay = 1; // **SIEMPRE día 1 por defecto**
        string defaultUpgrades = "";

        PlayerPrefs.SetFloat("PlayerMoney", defaultMoney);
        PlayerPrefs.SetFloat("SleepQuality", defaultSleep);
        PlayerPrefs.SetInt("CurrentDay", defaultDay);
        PlayerPrefs.SetString("PurchasedUpgrades", defaultUpgrades);
        PlayerPrefs.Save();

        UpdateGameVariables(defaultMoney, defaultSleep, defaultDay, defaultUpgrades);
        SaveUserDataToFirebase();

        Debug.Log($"✅ Datos por defecto creados - Día: {defaultDay}");
    }

    [ContextMenu("Guardar Datos en Firebase")]
    public void SaveGameData()
    {
        SaveUserDataToFirebase();
    }

    [ContextMenu("Cargar Datos de Firebase")]
    public void LoadGameData()
    {
        LoadUserDataFromFirebase();
    }

    public void ShowCurrentData()
    {
        float money = PlayerPrefs.GetFloat("PlayerMoney", 2500f);
        float sleep = PlayerPrefs.GetFloat("SleepQuality", 100f);
        int day = PlayerPrefs.GetInt("CurrentDay", 1);
        string upgrades = PlayerPrefs.GetString("PurchasedUpgrades", "");

        Debug.Log("=== DATOS ACTUALES ===");
        Debug.Log($"💰 Dinero: ${money:F2}");
        Debug.Log($"😴 Sueño: {sleep:F1}%");
        Debug.Log($"📅 Día: {day}");
        Debug.Log($"🛍️ Mejoras: {(string.IsNullOrEmpty(upgrades) ? "Ninguna" : upgrades)}");
    }

    [ContextMenu("Corregir Día a 1")]
    public void ForceFixDayToOne()
    {
        PlayerPrefs.SetInt("CurrentDay", 1);
        PlayerPrefs.Save();
        
        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.gameClock != null)
        {
            GameManagerPersistent.Instance.gameClock.SetExactTime(8, 0);
        }
        
        SaveUserDataToFirebase();
        
        Debug.Log("🔧 Día forzado a 1 y guardado en Firebase");
    }
}