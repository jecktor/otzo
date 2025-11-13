using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    private HashSet<string> purchasedUpgrades = new HashSet<string>();
    private const string PURCHASED_UPGRADES_KEY = "PurchasedUpgrades";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPurchasedUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PurchaseUpgrade(string upgradeName)
    {
        if (!purchasedUpgrades.Contains(upgradeName))
        {
            purchasedUpgrades.Add(upgradeName);
            SavePurchasedUpgrades();
            Debug.Log($"✅ Mejora '{upgradeName}' comprada y guardada");
        }
        else
        {
            Debug.LogWarning($"⚠️ La mejora '{upgradeName}' ya estaba comprada");
        }
    }

    public bool IsUpgradePurchased(string upgradeName)
    {
        return purchasedUpgrades.Contains(upgradeName);
    }

    public List<string> GetPurchasedUpgrades()
    {
        return new List<string>(purchasedUpgrades);
    }

    private void LoadPurchasedUpgrades()
    {
        string savedUpgrades = PlayerPrefs.GetString(PURCHASED_UPGRADES_KEY, "");
        if (!string.IsNullOrEmpty(savedUpgrades))
        {
            string[] upgrades = savedUpgrades.Split(';');
            foreach (string upgrade in upgrades)
            {
                if (!string.IsNullOrEmpty(upgrade))
                {
                    purchasedUpgrades.Add(upgrade);
                }
            }
        }
        Debug.Log($"📦 Mejoras cargadas: {purchasedUpgrades.Count}");
    }

    private void SavePurchasedUpgrades()
    {
        List<string> upgradesList = new List<string>(purchasedUpgrades);
        string savedUpgrades = string.Join(";", upgradesList.ToArray());
        PlayerPrefs.SetString(PURCHASED_UPGRADES_KEY, savedUpgrades);
        PlayerPrefs.Save();
    }

    // Método para debug
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("🎯 Mejoras compradas:");
            foreach (string upgrade in purchasedUpgrades)
            {
                Debug.Log($" - {upgrade}");
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ResetPurchases();
        }
    }

    public void ResetPurchases()
    {
        purchasedUpgrades.Clear();
        PlayerPrefs.DeleteKey(PURCHASED_UPGRADES_KEY);
        PlayerPrefs.Save();
        Debug.Log("🔄 Todas las compras reseteadas");
    }
}