using UnityEngine;

public class EmailManager : MonoBehaviour
{
    public static EmailManager Instance { get; private set; }

    public bool HasReadFirstDayEmail { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            HasReadFirstDayEmail = PlayerPrefs.GetInt("HasReadFirstDayEmail", 0) == 1;
            Debug.Log($"📧 Estado correo cargado: {(HasReadFirstDayEmail ? "LEÍDO" : "NO LEÍDO")}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkEmailAsRead()
    {
        HasReadFirstDayEmail = true;
        PlayerPrefs.SetInt("HasReadFirstDayEmail", 1);
        PlayerPrefs.Save();
        Debug.Log("✅ Correo marcado como leído y guardado en PlayerPrefs");
    }

    public void ResetEmailState()
    {
        HasReadFirstDayEmail = false;
        PlayerPrefs.SetInt("HasReadFirstDayEmail", 0);
        PlayerPrefs.Save();
        Debug.Log("🔄 Estado del correo reseteado");
    }

    public void PrintEmailState()
    {
        Debug.Log($"📧 Estado actual: {(HasReadFirstDayEmail ? "LEÍDO" : "NO LEÍDO")} | PlayerPrefs: {PlayerPrefs.GetInt("HasReadFirstDayEmail", 0)}");
    }
}