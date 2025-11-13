using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    [Header("UI Display")]
    public TextMeshProUGUI moneyText;

    // VARIABLE ESTÁTICA - Persiste sin necesidad de Singleton complejo
    public static float totalMoney = 2500f;

    void Start()
    {
        // Cargar dinero guardado
        totalMoney = PlayerPrefs.GetFloat("PlayerMoney", 2500f);
        UpdateDisplay();
    }

    void Update()
    {
        // Actualizar UI constantemente
        if (moneyText != null)
            moneyText.text = $"${totalMoney:F2}";
    }

    public void AddMoney(float amount)
    {
        totalMoney += amount;
        SaveMoney();
    }

    public bool SpendMoney(float amount)
    {
        if (totalMoney >= amount)
        {
            totalMoney -= amount;
            SaveMoney();
            return true;
        }
        return false;
    }

    void SaveMoney()
    {
        PlayerPrefs.SetFloat("PlayerMoney", totalMoney);
        PlayerPrefs.Save();
    }

    void UpdateDisplay()
    {
        if (moneyText != null)
            moneyText.text = $"${totalMoney:F2}";
    }

    void OnApplicationQuit()
    {
        SaveMoney();
    }
}