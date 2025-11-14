using UnityEngine;

public class GlobalUser : MonoBehaviour
{
	public static GlobalUser Instance { get; private set; }

	public string Username { get; private set; }
	public int BestScore { get; private set; }

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
        
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void SetUser(string username, int bestScore)
	{
		Username = username;
		BestScore = bestScore;
	}

	public void UpdateBestScore(int newScore)
	{
		if (newScore > BestScore)
			BestScore = newScore;
	}
}
