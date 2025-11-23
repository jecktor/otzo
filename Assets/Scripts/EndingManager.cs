using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndingManager : MonoBehaviour
{
	[Header("Panels")]
	public GameObject galleryPanel;
	public GameObject cutscenePanel;
	public GameObject bg;

	[Header("Cutscene UI")]
	public RawImage cutsceneRawImage;
	public CanvasGroup cutsceneCanvasGroup;

	[Header("Cutscene textures")]
	public Texture2D[] finalEmprendedorTextures;
	public Texture2D[] finalSecretoTextures;

	[Header("Settings")]
	public float fadeDuration = 0.35f;

	private Texture2D[] currentSequence;
	private int currentIndex = 0;
	private bool isPlaying = false;
	private bool isTransitioning = false;

	// --- Called by Gallery button ---
	public void OpenGalleryPanel()
	{
		galleryPanel.SetActive(true);
	}

	// --- Called by X button ---
	public void CloseGalleryPanel()
	{
		galleryPanel.SetActive(false);
	}

	// --- Button: Final Emprendedor ---
	public void PlayFinalEmprendedor()
	{
		StartCutscene(finalEmprendedorTextures);
	}

	// --- Button: Final Secreto ---
	public void PlayFinalSecreto()
	{
		StartCutscene(finalSecretoTextures);
	}

	// --- Start a cutscene sequence ---
	void StartCutscene(Texture2D[] sequence)
	{
		if (sequence == null || sequence.Length == 0)
			return;

		currentSequence = sequence;
		currentIndex = 0;
		isPlaying = true;

		// Show UI

		bg.SetActive(true);
		cutscenePanel.SetActive(true);

		// Start fully invisible
		cutsceneCanvasGroup.alpha = 0f;
		cutsceneRawImage.texture = currentSequence[currentIndex];

		// Fade IN the first slide
		StartCoroutine(FadeInCutscene());
	}

	// --- Click to continue ---
	public void OnCutsceneClick()
	{
		if (!isPlaying || isTransitioning) return;

		currentIndex++;

		// Last slide completed
		if (currentIndex >= currentSequence.Length)
		{
			StartCoroutine(FadeOutAndEnd());
			return;
		}

		// Fade to the next slide
		StartCoroutine(FadeToTexture(currentSequence[currentIndex]));
	}

	// --- Fade IN at start ---
	IEnumerator FadeInCutscene()
	{
		isTransitioning = true;

		float t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			cutsceneCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
			yield return null;
		}

		cutsceneCanvasGroup.alpha = 1f;
		isTransitioning = false;
	}

	// --- Fade OUT at the end ---
	IEnumerator FadeOutAndEnd()
	{
		isTransitioning = true;

		float t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			cutsceneCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
			yield return null;
		}

		cutsceneCanvasGroup.alpha = 0f;

		// Now fully invisible → end cutscene
		EndCutscene();
		isTransitioning = false;
	}

	// --- End cutscene ---
	void EndCutscene()
	{
		isPlaying = false;

		cutscenePanel.SetActive(false);
		bg.SetActive(false);

		galleryPanel.SetActive(true);
	}

	// --- Fade transition between slides ---
	IEnumerator FadeToTexture(Texture2D nextTexture)
	{
		isTransitioning = true;

		// Fade OUT
		float t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			cutsceneCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
			yield return null;
		}

		// Swap image
		cutsceneRawImage.texture = nextTexture;

		// Fade IN
		t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			cutsceneCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
			yield return null;
		}

		cutsceneCanvasGroup.alpha = 1f;
		isTransitioning = false;
	}
}
