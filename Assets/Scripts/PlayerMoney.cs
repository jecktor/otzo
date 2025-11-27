using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    [Header("UI Display")]
	public TextMeshProUGUI moneyText;
	public bool keepMoney = true;

    public static float totalMoney = 0f;

    void Start()
    {
	    totalMoney = keepMoney ? PlayerPrefs.GetFloat("PlayerMoney", 0f) : 0f;
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
		if (!keepMoney) return;
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

    [ContextMenu("Reset Money")]
    public void ResetMoney()
    {
        totalMoney = 0f;
        SaveMoney();
        UpdateDisplay();
    }
}