using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
            if (positions[0].Count <= 0) return distance;

            var old = new Vector3(positions[0][0], positions[1][0], positions[2][0]);
            for (var i = 1; i < positions[0].Count; i++)
            {
                var current = new Vector3(positions[0][i], positions[1][i], positions[2][i]);
                distance += (current - old).magnitude;
                old = current;
            }
            return distance;
        }

        /// <summary>
        /// Computes the length of a message in pixel.
        /// </summary>
        /// <param name="message">String to be messured</param>
        /// <param name="tex">Text element providing formatting</param>
        /// <returns>Length in pixel</returns>
        public static int MessagePixelLength(string message, Text tex)
        {
            var totalLength = 0;
            var myFont = tex.font;
            var arr = message.ToCharArray();

            myFont.RequestCharactersInTexture(message, tex.fontSize, tex.fontStyle);
            for (var index = 0; index < arr.Length; index++)
            {
                CharacterInfo characterInfo;
                myFont.GetCharacterInfo(arr[index], out characterInfo, tex.fontSize);
                totalLength += characterInfo.advance;
            }

            return totalLength;
        }

        /// <summary>
        /// Compares text length of multiple labels and returns maximal length in pixel.
        /// </summary>
        /// <param name="labels">Labels to be compared</param>
        /// <param name="min">Optional: changes the minimal text length returned.</param>
        /// <returns>Maximal length in pixel</returns>
        public static int GetMaxTextLength(IEnumerable<string> labels, int min = 0)
        {
            var labelTextTmp = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Questionnaire/Rows/Elements/OneLabel");
            var textTmp = labelTextTmp.GetComponent<Text>();

            var maxLength = labels.Select(t => MessagePixelLength(t, textTmp)).Concat(new[] { min }).Max();

            Object.Destroy(textTmp);
            Object.Destroy(labelTextTmp);
            return maxLength;
        }

        /// <summary>
        /// Computes the size of a label that may break into multiple lines
        /// </summary>
        /// <param name="labels">Labels to compute size</param>
        /// <param name="rowHeight">Height of one row in pixels.</param>
        /// <param name="minLabelWidth">Minimum width of label in pixels</param>
        /// <returns>Height and width accommodating all labels</returns>
        public static Vector2 ComputeTopLabelSize(IList<string> labels, int rowHeight = 32, int minLabelWidth = 150)
        {
            float minWidth = minLabelWidth;
            minWidth = labels.Aggregate(minWidth, (current, label) => new[] {GetMaxTextLength(label.Split(' '), minLabelWidth), current}.Max());
            
            var fullLength = GetMaxTextLength(labels);
            var nlines = (int) Math.Ceiling(fullLength / minWidth);

            var height = rowHeight;
            if (nlines * rowHeight > height)
                height = nlines * rowHeight;

            return new Vector2(minWidth, height);
        }


        /// <summary>
        /// Adds a label element to a question.
        /// </summary>
        /// <param name="labelType">Name of Prefab to be instantiated</param>
        /// <param name="labelText">Text on Label</param>
        /// <param name="parent">Container where to place label</param>
        /// <returns>Returns the new label</returns>
        public static GameObject AddLabelText(string labelType, string labelText, Transform parent)
        {
            var label = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + labelType);
            PlaceElement(label, parent);
            label.GetComponent<Text>().text = labelText;
            return label;
        }

        /// <summary>
        /// Adds a sub label element to a question.
        /// </summary>
        /// <param name="labelType">Name of Prefab to be instantiated</param>
        /// <param name="labelText">Text on sublabel</param>
        /// <param name="parent">Container where to place label</param>
        /// <param name="subLabelType">Container within the Prefab where the sublabel is found</param>
        /// <param name="subLabelLength">Length of sublabel</param>
        /// <returns></returns>
        public static GameObject AddLabelText(string labelType, string labelText, Transform parent, string subLabelType, float subLabelLength)
        {
            var label = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + labelType);
            PlaceElement(label, parent);
            var sublabel = label.transform.Find(subLabelType);

            sublabel.GetComponent<RectTransform>().sizeDelta = new Vector2(subLabelLength, 50);
            sublabel.GetComponent<Text>().text = labelText;
            return label;
        }

        /// <summary>
        /// Creates a toggle with a width and height.
        /// </summary>
        /// <param name="toggleType">Toggle type to be created.</param>
        /// <param name="parent">Container where to place toggle</param>
        /// <param name="width">Width of toggle area</param>
        /// <param name="height">Height of toggle area</param>
        /// <returns>Transform of created toggle</returns>
        public static Transform CreateToggleElement(string toggleType, Transform parent, float width, float height)
        {
            var toggleObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + toggleType);
            toggleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            PlaceElement(toggleObject, parent);
            var button = toggleObject.transform.Find("ToggleButtons");
            return button;
        }
    }
}
