using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirestoreTest : MonoBehaviour
{
	FirebaseFirestore db;

	async void Start()
	{
		// Initialize Firebase
		var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
		await dependencyTask;

		if (dependencyTask.Result != DependencyStatus.Available)
		{
			Debug.LogError("Firebase init failed: " + dependencyTask.Result);
			return;
		}

		Debug.Log("Firebase READY!");

		db = FirebaseFirestore.DefaultInstance;

		await TestWrite();
		await TestRead();
	}

	async System.Threading.Tasks.Task TestWrite()
	{
		DocumentReference doc = db.Collection("testCollection").Document("testDoc");

		Dictionary<string, object> data = new Dictionary<string, object>
		{
			{ "username", "papayin" },
			{ "score", 999 },
			{ "timestamp", Timestamp.GetCurrentTimestamp() }
		};

		await doc.SetAsync(data);
		Debug.Log("Firestore WRITE ✔ Completed");
	}

	async System.Threading.Tasks.Task TestRead()
	{
		DocumentReference doc = db.Collection("testCollection").Document("testDoc");
		DocumentSnapshot snap = await doc.GetSnapshotAsync();

		if (snap.Exists)
		{
			Debug.Log("Firestore READ ✔ Success");
			Debug.Log("Data: " + snap.ConvertTo<Dictionary<string, object>>());
		}
		else
		{
			Debug.LogError("Document not found.");
		}
	}
}
