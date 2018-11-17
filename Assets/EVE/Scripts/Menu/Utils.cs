using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu
{
    public static class Utils {
        
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
    }
}
