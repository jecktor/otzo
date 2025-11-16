using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    [Header("UI Display")]
    public TextMeshProUGUI moneyText;

    public static float totalMoney = 2500f;

    void Start()
    {
        totalMoney = PlayerPrefs.GetFloat("PlayerMoney", 2500f);
        UpdateDisplay();
    }

    void Update()
    {
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