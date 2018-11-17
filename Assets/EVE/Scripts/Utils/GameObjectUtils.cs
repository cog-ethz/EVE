using System.Collections;
using UnityEngine;

public static class GameObjectUtils {

    /// <summary>
    /// Destroys a GameObject with a delay
    /// </summary>
    /// <param name="gameObject">To be destroyed</param>
    /// <param name="delay">Time to destruction</param>
    /// <returns>Coroutine to execute the delay</returns>
	public static IEnumerator RemoveGameObject(GameObject gameObject, int delay = 0)
	{
	    yield return new WaitForSeconds(delay);
	    Object.Destroy(gameObject);
	}

}
