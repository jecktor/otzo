using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour
{
	[Header("Panels")]
	public GameObject galleryPanel;

	[Header("Grid setup")]
	public Transform gridParent;

	[Header("Images")]
	public Texture2D[] textures;   // <<< Use Texture2D instead of Sprite[]

	[Header("Fullscreen")]
	public GameObject fullscreenPanel;
	public RawImage fullscreenImage;

	bool initialized = false;

	void Start()
	{
		galleryPanel.SetActive(false);
		fullscreenPanel.SetActive(false);
	}

	public void OpenGallery()
	{
		galleryPanel.SetActive(true);

		if (!initialized)
		{
			initialized = true;
		}
	}

	public void ShowFullscreen(Texture2D tex)
	{
		fullscreenPanel.SetActive(true);

		fullscreenImage.texture = tex;
	}

	public void CloseFullscreen()
	{
		fullscreenPanel.SetActive(false);
	}

	public void CloseGallery()
	{
		fullscreenPanel.SetActive(false);
		galleryPanel.SetActive(false);
	}
}
