using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using EasyPeasyFirstPersonController;

public class ScanMiniGame : MonoBehaviour
{
	[Header("Gameplay settings")]
	public KeyCode scanKey = KeyCode.E;
	public float baseWindow = 1.2f;         // start easy (1.2 sec to hit)
	public float minWindow = 0.4f;          // hardest timing window
	public float windowDecayRate = 0.02f;   // how much to shrink window per scan
	public float delayBetweenItems = 0.4f;
	public float diffMult = 0.3f;
	public int totalScans = 0;

	private float currentWindow;
	private int successfulScans;
	
	public SkillCheckUI skillCheckUI;
	public CustomerSpawner cs;
	public FirstPersonController player;
	
	private float hardScanChance = 0.5f;    // starts at 10%
	private float hardScanChanceIncrease = 0.05f; // +5% each normal scan


	void Awake()
	{
		currentWindow = baseWindow;
	}
	
	public void StepDownDifficulty()
	{
		diffMult -= 0.2f;
	}

	public void Stop()
	{
		// Immediately stop any running mini-game logic
		StopAllCoroutines();

		// Hide any visible UI (skill check, feedback text, etc.)
		if (skillCheckUI != null)
			skillCheckUI.Hide();

		Debug.Log("[ScanMiniGame] Force-stopped mini-game.");
	}


	/// <summary>
	/// Runs the scanning mini-game.
	/// </summary>
	public IEnumerator Run(List<GameObject> items, float totalValue, System.Action<float> onComplete)
	{
		if (cs == null || player == null || cs.IsMeltdownInProgress)
			yield break;
		
		if (items == null || items.Count == 0)
		{
			onComplete?.Invoke(0f);
			yield break;
		}
		
		player.SetControl(false);

		totalScans++;
		successfulScans = 0;
		float earned = 0f;

		// progressively tighten window
		currentWindow = Mathf.Max(minWindow, baseWindow - totalScans * windowDecayRate);

		for (int i = 0; i < items.Count; i++)
		{
			GameObject item = items[i];
			item.SetActive(true);

			// --- Wait for timing window ---
			float timer = currentWindow;
			// --- Inside Run() loop, for each item ---
			bool scanned = false;

			bool isHardScan = Random.value < hardScanChance;
			
			if (isHardScan)
			{
				Debug.Log("Hard scan");
				skillCheckUI.Show(diffMult + 0.3f);
				hardScanChance = 0.1f;
			}
			else
			{
				skillCheckUI.Show(diffMult);
				hardScanChance = Mathf.Min(1f, hardScanChance + hardScanChanceIncrease);
			}

			int attempts = 1;

			while (!scanned)
			{
			    yield return null;
			
			    if (skillCheckUI.GetResult())
			    {
				    scanned = true;
				    successfulScans++;
				    earned += totalValue / items.Count;
				    skillCheckUI.Hide();
			    }
			
			    // If player failed (pressed outside zone)
			    if (!skillCheckUI.GetResult() && Input.GetKeyDown(KeyCode.E))
			    {
				    skillCheckUI.Stop();
				    yield return new WaitForSeconds(2f);
				    if (isHardScan)
				    {
					    Debug.Log("No escape");
					    skillCheckUI.Show((diffMult + 0.3f) - (0.02f * attempts));
					    hardScanChance = 0.1f;
				    }
				    else
				    {
					    skillCheckUI.Show(diffMult);
					    hardScanChance = Mathf.Min(1f, hardScanChance + hardScanChanceIncrease);
				    }
				    
				    attempts++; 
			    }
			}
			
			diffMult = Mathf.Clamp(0.3f + totalScans * 0.03f, 0.3f, 1.3f);

			item.SetActive(false);
			yield return new WaitForSeconds(delayBetweenItems);
		}

		float accuracy = (float)successfulScans / items.Count;
		float payout = totalValue * accuracy;

		onComplete?.Invoke(payout);
		
		player.SetControl(true);
	}
}
