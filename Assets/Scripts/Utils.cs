using System.Collections;
using UnityEngine;

public static class CoroutineUtils
{
	/// <summary>
	/// Waits until one of two conditions: the predicate returns true or a timeout expires.
	/// </summary>
	public static IEnumerator WaitForEither(System.Func<bool> condition, float timeout)
	{
		float timer = 0f;
		while (timer < timeout)
		{
			if (condition())
				yield break; // condition met first → exit immediately

			timer += Time.deltaTime;
			yield return null;
		}
		// timeout reached
	}
}
