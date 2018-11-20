using System.Collections;
using UnityEngine;

namespace Assets.EVE.Scripts.Utils
{
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


        /// <summary>
        /// Instantiates prefab and asserts that it exists
        /// </summary>
        /// <param name="prefab">Prefab to be instantiated</param>
        /// <returns></returns>
        public static GameObject InstatiatePrefab(string prefab)
        {
            var newObject = Object.Instantiate(Resources.Load(prefab)) as GameObject;
            if (newObject == null) Debug.LogError("Failed instantiating " + prefab);
            return newObject;
        }
    }
}
