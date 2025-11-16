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

			// If user does NOT exist
			if (!task.Result.Exists)
			{
				CreateNewUser(username);
				return;
			}

			// User exists → load data
			Dictionary<string, object> data = task.Result.ToDictionary();

			int score = 0;
			if (data.ContainsKey("bestScore"))
			{
				int.TryParse(data["bestScore"].ToString(), out score);
			}

			// Store globally
			GlobalUser.Instance.SetUser(username, score);

			feedbackText.text = $"Bienvenido de vuelta, {username}!";

			// TODO: Load your main menu or game scene
		});
	}

	void CreateNewUser(string username)
	{
		var data = new Dictionary<string, object>
		{
			{"username", username},
			{"bestScore", 0}
		};

		DocumentReference doc = db.Collection("players").Document(username);

		doc.SetAsync(data).ContinueWithOnMainThread(task =>
		{
			if (task.IsFaulted)
			{
				feedbackText.text = "Error creando usuario.";
				return;
			}

			// Store globally
			GlobalUser.Instance.SetUser(username, 0);

			feedbackText.text = $"¡Usuario creado: {username}!";

			// TODO: Load your main menu or game scene
		});
	}
}
