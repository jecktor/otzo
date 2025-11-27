using UnityEngine;
using UnityEngine.UI;

public class GalleryItemUI : MonoBehaviour
{
	public Texture2D texture;
	public GalleryManager galleryManager;

	public void OnClick()
	{
		if (galleryManager != null)
			galleryManager.ShowFullscreen(texture);
	}
}
