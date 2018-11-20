using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.EVE.Scripts.Utils
{
    public static class MenuUtils {
        
        /// <summary>
        /// Moves a new menu element within the layout into its position
        /// </summary>
        /// <param name="element">Element to be placed</param>
        /// <param name="parent">Container for the Element</param>
        public static void PlaceElement(GameObject element, Transform parent)
        {
            element.transform.SetParent(parent.transform);
            element.transform.localPosition = new Vector3(element.transform.localPosition.x, element.transform.localPosition.y, parent.localPosition.z);
            element.transform.localScale = new Vector3(1, 1, 1);
        }

        /// <summary>
        /// Method removes all children from a game object transform representing
        /// a list in a menu
        /// </summary>
        /// <param name="listContainer">Container for list elements</param>
        public static void ClearList(Transform listContainer)
        {
            foreach (Transform child in listContainer)
            {
                Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Sums the distances between all stored locations of a participants path.
        /// </summary>
        /// <param name="positions">X,Y,Z Coordiantes of a participant.</param>
        /// <returns></returns>
        public static float ComputeParticipantPathDistance(List<float>[] positions)
        {
            var distance = 0f;
            var old = new Vector3(positions[0][0], positions[1][0], positions[2][0]);
            for (var i = 1; i < positions[0].Count; i++)
            {
                var current = new Vector3(positions[0][i], positions[1][i], positions[2][i]);
                distance += (current - old).magnitude;
                old = current;
            }
            return distance;
        }

    }
}
