using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class UsernameLogin : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField usernameInput;
    public TMP_Text feedbackText;

    FirebaseFirestore db;

    async void Start()
    {
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dep != DependencyStatus.Available)
        {
            feedbackText.text = "Error inicializando Firebase.";
            return;
        }

        db = FirebaseFirestore.DefaultInstance;
    }

    public void OnConfirmUsername()
    {
        string username = usernameInput.text.Trim().ToLower();

        if (string.IsNullOrEmpty(username))
        {
            feedbackText.text = "Introduce un nombre válido.";
            return;
        }

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
                return;
            }

            if (!task.Result.Exists)
            {
                CreateNewUser(username);
                return;
            }

            LoadExistingUser(username);
        });
    }

    void CreateNewUser(string username)
    {
        GlobalUser.Instance.SetUser(username, 0);

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.CreateDefaultUserData();
        }

        feedbackText.text = $"¡Usuario creado: {username}!";

        // TODO: Load your main menu or game scene
    }

    void LoadExistingUser(string username)
    {
        GlobalUser.Instance.SetUser(username, 0);

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.LoadUserDataFromFirebase();
        }

        feedbackText.text = $"Bienvenido de vuelta, {username}!";

        // TODO: Load your main menu or game scene

    }
}