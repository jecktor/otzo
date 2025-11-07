using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillCheckUI : MonoBehaviour
{
	[Header("UI Elements")]
	public RectTransform pointer;
	public RectTransform hitZone;
	public RectTransform barArea; // the full width reference (BackgroundBar)

	[Header("Gameplay Settings")]
	public CustomerSpawner cs;
	public float baseSpeed = 250f;
	public Color successColor = Color.green;
	public Color failColor = Color.red;

	private bool active = false;
	private bool success;
	private bool goingRight = true;
	private float barWidth;
	private float precisionFactor;

	void Start() => Hide();

	public void Show(float difficultyMultiplier = 1f)
	{
		if (cs == null || cs.IsMeltdownInProgress)
			return;
		
		hitZone.GetComponent<Image>().color = successColor;
		gameObject.SetActive(true);
		active = true;
		success = false;

		barWidth = barArea.rect.width;
		pointer.anchoredPosition = new Vector2(-barWidth / 2f, 0);

		// randomize hit zone position
		float randomX = Random.Range(-barWidth / 3f, barWidth / 3f);
		hitZone.anchoredPosition = new Vector2(randomX, 0);
		
		precisionFactor = 1 / (10 * difficultyMultiplier);
		
		Vector3 scale = hitZone.localScale;
		scale.x = precisionFactor;
		hitZone.localScale = scale;

		goingRight = true;
	}
	
	public void Stop()
	{
		hitZone.GetComponent<Image>().color = failColor;
		active = false;
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		active = false;
	}

	void Update()
	{
		if (!active) return;

		float move = baseSpeed * Time.deltaTime * (goingRight ? 1 : -1) * (0.1f / precisionFactor);
		pointer.anchoredPosition += new Vector2(move, 0);

		// bounce when hitting edges
		if (pointer.anchoredPosition.x > barWidth / 2f)
			goingRight = false;
		else if (pointer.anchoredPosition.x < -barWidth / 2f)
			goingRight = true;


		if (Input.GetKeyDown(KeyCode.E))
		{
			float pointerX = pointer.localPosition.x;
			float zoneX = hitZone.localPosition.x;
			float halfZoneWidth = hitZone.rect.width * 0.5f;

			// Define a tolerance multiplier (tightens or widens precision)

			float zoneMin = zoneX - halfZoneWidth * precisionFactor;
			float zoneMax = zoneX + halfZoneWidth * precisionFactor;

			// Check if pointer is truly inside the visible zone
			if (pointerX >= zoneMin && pointerX <= zoneMax)
			{
			    success = true;
			}
			else
			{
			    success = false;
			}
		}
	}

	public bool GetResult() => success;
}
