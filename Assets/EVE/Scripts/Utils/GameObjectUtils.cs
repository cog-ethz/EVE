using System.Collections;
using System.Linq;
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
        public static IEnumerator RemoveGameObject(GameObject gameObject, float delay = 0)
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
            Debug.Log("Instantiate " + prefab);
            var newObject = Object.Instantiate(Resources.Load(prefab)) as GameObject;
            if (newObject == null) Debug.LogError("Failed instantiating " + prefab);
            return newObject;
        }


        /// <summary>
        /// Find a named game object with depth first search.
        /// </summary>
        /// <remarks>
        /// Note that only the first game object of the name will be returned.
        /// </remarks>
        /// <param name="container">Root of hierarchy to be searched.</param>
        /// <param name="gameObjectName">Name of game object to be found.</param>
        /// <returns>First instance of a game object with requested name.</returns>
        public static Transform FindGameObjectInChildren(Transform container, string gameObjectName)
        {
            Transform dynf = null;
            var allGameObjects = container.GetComponentsInChildren<Transform>().ToList();
            foreach (var child in allGameObjects)
            {
                if (child.name == gameObjectName)
                {
                    dynf = child;
                    break;
                }
            }
            return dynf;
        }
        
        /// <summary>
        /// Gets the index of a game object within its parent.
        /// </summary>
        /// <param name="gameObject">GameObject to be found.</param>
        /// <returns>Index of GameObject.</returns>
        public static int GetIndexOfGameObject(GameObject gameObject)
        {
            var parent = gameObject.transform.parent;
            var numberOfEntry = -1;
            for (var index = 0; index < parent.childCount; index++)
            {
                var entryObject = parent.GetChild(index);
                if (entryObject.gameObject == gameObject)
                {
                    numberOfEntry = index;
                }
            }
            return numberOfEntry;
        }
    }
}
