using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using EasyPeasyFirstPersonController;

public class CustomerStressManager : MonoBehaviour
{
	[Header("Settings")]
	public float maxStress = 10f;              // time (seconds) to reach max stress
	public CustomerSpawner cs;         
	public Slider stressBar;                   // UI slider (0–1)
	public QueueManager qm;
	public ScanMiniGame smg;
	public TextMeshProUGUI mt;

	[Header("Visuals")]
	public Color normalColor = Color.green;
	public Color warningColor = Color.yellow;
	public Color dangerColor = Color.red;
	public string[] mentadas;

	private float stressLevel = 0f;
	private bool meltdownTriggered = false;
	private bool timerPlaying;
	
	private Image fillImage;
	public FirstPersonController player;
	
	public AudioSource timer;
	public AudioSource fail;
	

	void Start()
	{
		if (stressBar != null)
		{
			stressBar.minValue = 0f;
			stressBar.maxValue = 1f;
			fillImage = stressBar.fillRect.GetComponent<Image>();
		}
	}

	void Update()
	{
		if (cs == null || qm == null || smg == null || meltdownTriggered) return;

		if (qm.IsQueueFull && cs.IsStoreFull)
		{
			if (!timerPlaying && timer != null)
			{
				timer.Play();
				timerPlaying = true;
			}

			stressLevel += Time.deltaTime;
			if (stressLevel >= maxStress)
			{
				meltdownTriggered = true;
				StartCoroutine(TriggerMeltdown());
			}
		}
		else
		{
			if (timerPlaying && timer != null)
			{
				timer.Stop();
				timerPlaying = false;
			}
			// relieve stress when queue is under control
			stressLevel = Mathf.Max(0f, stressLevel - Time.deltaTime * 1.5f);
		}

		UpdateUI();
	}

	void UpdateUI()
	{
		if (stressBar == null) return;
		float ratio = stressLevel / maxStress;
		stressBar.value = ratio;

		if (fillImage != null)
		{
			if (ratio < 0.5f) fillImage.color = normalColor;
			else if (ratio < 0.8f) fillImage.color = warningColor;
			else fillImage.color = dangerColor;
		}
	}

	IEnumerator TriggerMeltdown()
	{
		Debug.Log("Customer meltdown! Everyone leaves!");
		player.SetControl(true);
		
		fail.Play();
		
		stressLevel = maxStress;
		UpdateUI();
		mt.text = mentadas[Random.Range(0, mentadas.Length)];

		smg.Stop();
		cs.ClearAllCustomers();
		
		smg.StepDownDifficulty();
		
		yield return new WaitForSeconds(cs.meltdownPenalty);
		
		meltdownTriggered = false;
		stressLevel = 0f;
		UpdateUI();
		mt.text = "";
	}

	public void ResetStress()
	{
		stressLevel = 0f;
		meltdownTriggered = false;
		UpdateUI();
	}
}
