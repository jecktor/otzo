using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class RandomSkin : MonoBehaviour
{
	public Texture2D[] availableSkins; // drag all your textures here in Inspector

	void Start()
	{
		if (availableSkins == null || availableSkins.Length == 0) return;

		// Pick a random texture
		Texture2D selected = availableSkins[Random.Range(0, availableSkins.Length)];

		// Get the mesh renderer and its material instance
		var renderer = GetComponent<SkinnedMeshRenderer>();
		Material mat = renderer.material; // creates a unique material instance

		// Apply texture to the Albedo (Base Map)
		mat.mainTexture = selected;
	}
}
