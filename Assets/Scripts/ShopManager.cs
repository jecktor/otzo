using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    }

    private void SavePurchasedUpgrades()
    {
        List<string> upgradesList = new List<string>(purchasedUpgrades);
        string savedUpgrades = string.Join(";", upgradesList.ToArray());
        PlayerPrefs.SetString(PURCHASED_UPGRADES_KEY, savedUpgrades);
        PlayerPrefs.Save();
    }

    public string GetPurchasedUpgradesString()
    {
        return string.Join(";", purchasedUpgrades.ToArray());
    }

    public void ResetPurchases()
    {
        purchasedUpgrades.Clear();
        PlayerPrefs.DeleteKey(PURCHASED_UPGRADES_KEY);
        PlayerPrefs.Save();
    }
}