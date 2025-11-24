using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UsernameLogin : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField usernameInput;
    public TMP_Text feedbackText;

    private FirebaseFirestore db;
    private bool isProcessing = false;

    async void Start()
    {
        feedbackText.text = "Inicializando...";

        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dep != DependencyStatus.Available)
        {
            feedbackText.text = "Error inicializando Firebase.";
            return;
        }

        db = FirebaseFirestore.DefaultInstance;
        feedbackText.text = "Listo. Introduce tu nombre de usuario.";
    }

    public void OnConfirmUsername()
    {
        // Evitar múltiples clics mientras se procesa
        if (isProcessing)
        {
            feedbackText.text = "Procesando, espera...";
            return;
        }

        string username = usernameInput.text.Trim().ToLower();

        if (string.IsNullOrEmpty(username))
        {
            feedbackText.text = "Introduce un nombre válido.";
            return;
        }

        if (username.Length < 3)
        {
            feedbackText.text = "El nombre debe tener al menos 3 caracteres.";
            return;
        }

        // Marcar como procesando y cambiar texto
        isProcessing = true;
        feedbackText.text = "Cargando...";

        CheckIfUsernameExists(username);
    }

    void CheckIfUsernameExists(string username)
    {
        DocumentReference doc = db.Collection("players").Document(username);

        doc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                feedbackText.text = "Error conectando con Firestore.";
                isProcessing = false;
                return;
            }

            if (task.IsCanceled)
            {
                feedbackText.text = "Operación cancelada. Intenta nuevamente.";
                isProcessing = false;
                return;
            }

            if (!task.Result.Exists)
            {
                CreateNewUser(username);
            }
            else
            {
                LoadExistingUser(username);
            }
        });
    }

    void CreateNewUser(string username)
    {
        try
        {
            feedbackText.text = "Creando usuario...";

            GlobalUser.Instance.SetUser(username, 0);

            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.CreateDefaultUserData();
            }

            feedbackText.text = $"¡Usuario {username} creado!";
            Debug.Log($"✅ Nuevo usuario creado: {username}");

            // Cargar menú principal después de un breve delay
            Invoke("LoadMainMenu", 1.5f);
        }
        catch (System.Exception ex)
        {
            feedbackText.text = "❌ Error creando usuario.";
            isProcessing = false;
            Debug.LogError($"Error creando usuario: {ex.Message}");
        }
    }

    void LoadExistingUser(string username)
    {
        try
        {
            feedbackText.text = "Cargando datos...";

            GlobalUser.Instance.SetUser(username, 0);

            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.LoadUserDataFromFirebase();
            }

            feedbackText.text = $"¡Bienvenido de vuelta, {username}!";
            Debug.Log($"Usuario cargado: {username}");

            // Cargar menú principal después de un breve delay
            Invoke("LoadMainMenu", 1.5f);
        }
        catch (System.Exception ex)
        {
            feedbackText.text = "❌ Error cargando usuario.";
            isProcessing = false;
            Debug.LogError($"Error cargando usuario: {ex.Message}");
        }
    }

    void LoadMainMenu()
    {
        try
        {
            feedbackText.text = "Cargando menú principal...";
            SceneManager.LoadScene("MainMenu");
        }
        catch (System.Exception ex)
        {
            feedbackText.text = "❌ Error cargando menú.";
            isProcessing = false;
            Debug.LogError($"Error cargando menú principal: {ex.Message}");
        }
    }

    // Método para reiniciar el estado si hay problemas
    public void ResetLoginState()
    {
        isProcessing = false;
        feedbackText.text = "Estado reiniciado. Introduce tu nombre de usuario.";
    }

    // Permitir usar Enter para confirmar
    void Update()
    {
        if (usernameInput != null && usernameInput.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnConfirmUsername();
        }
    }

    // Limpiar cuando se desactive el objeto
    void OnDisable()
    {
        isProcessing = false;
        CancelInvoke();
    }
}