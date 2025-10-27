using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    private HashSet<string> purchasedUpgrades = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

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
            Debug.Log($"Mejora comprada: {upgradeName}");
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

    public bool HasCustomerAttitudeUpgrade()
    {
        return IsUpgradePurchased("🌟 MEJORAR ACTITUD CLIENTES");
    }

    public bool HasInventoryExpansion()
    {
        return IsUpgradePurchased("🔒 EXPANSIÓN DE INVENTARIO");
    }

    public bool HasLoyaltySystem()
    {
        return IsUpgradePurchased("🔒 SISTEMA DE FIDELIDAD");
    }

    public bool HasDigitalMarketing()
    {
        return IsUpgradePurchased("🔒 MARKETING DIGITAL");
    }

    public bool HasDeliveryService()
    {
        return IsUpgradePurchased("🔒 SERVICIO A DOMICILIO");
    }
}