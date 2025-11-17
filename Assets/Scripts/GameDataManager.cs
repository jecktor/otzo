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
            Debug.Log("✅ Firebase inicializado");
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
            Debug.LogWarning("⚠️ No hay usuario logueado");
            return;
        }

        try
        {
            float money = PlayerPrefs.GetFloat("PlayerMoney", 2500f);
            float sleepQuality = PlayerPrefs.GetFloat("SleepQuality", 100f);
            int currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
            string upgrades = PlayerPrefs.GetString("PurchasedUpgrades", "");

            var data = new Dictionary<string, object>
            {
                { "username", GlobalUser.Instance.Username },
                { "bestScore", money },
                { "sleepQuality", sleepQuality },
                { "dayGame", currentDay },
                { "upgrades", upgrades },
            };

            DocumentReference docRef = db.Collection("players").Document(GlobalUser.Instance.Username);
            await docRef.SetAsync(data, SetOptions.MergeAll);

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
            Debug.LogWarning("⚠️ No hay usuario logueado");
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

                PlayerPrefs.SetFloat("PlayerMoney", money);
                PlayerPrefs.SetFloat("SleepQuality", sleepQuality);
                PlayerPrefs.SetInt("CurrentDay", currentDay);
                PlayerPrefs.SetString("PurchasedUpgrades", upgrades);
                PlayerPrefs.Save();

                UpdateGameVariables(money, sleepQuality, currentDay, upgrades);

                Debug.Log($"💰 ${money} | 😴 {sleepQuality}% | 📅 {currentDay} | 🛍️ {upgrades}");
            }
            else
            {
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
        PlayerWallet.totalMoney = money;

        if (GameManagerPersistent.Instance != null && GameManagerPersistent.Instance.sleepSystem != null)
        {
            GameManagerPersistent.Instance.sleepSystem.ModifySleepQuality(sleepQuality - GameManagerPersistent.Instance.sleepSystem.CurrentSleepQuality);
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

    }

    public void CreateDefaultUserData()
    {
        float defaultMoney = 2500f;
        float defaultSleep = 100f;
        int defaultDay = 1;
        string defaultUpgrades = "";

        PlayerPrefs.SetFloat("PlayerMoney", defaultMoney);
        PlayerPrefs.SetFloat("SleepQuality", defaultSleep);
        PlayerPrefs.SetInt("CurrentDay", defaultDay);
        PlayerPrefs.SetString("PurchasedUpgrades", defaultUpgrades);
        PlayerPrefs.Save();

        UpdateGameVariables(defaultMoney, defaultSleep, defaultDay, defaultUpgrades);

        SaveUserDataToFirebase();

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
}