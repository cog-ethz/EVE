using Assets.EVE.Scripts.Utils;
using UnityEngine;

namespace Assets.EVE.Scripts.Menu
{
    public class VariableInputFieldSize : MonoBehaviour {

        public UnityEngine.UI.InputField inputField;
        private int nLines = 0, oldNlines = 0;

        private int fieldWidth;

        void Start()
        {
            fieldWidth = (int) inputField.GetComponent<RectTransform>().sizeDelta.x;
            AdaptInputFieldSize();
        }

        /// <summary>
        /// Updates the size of the input field.
        /// </summary>
        public void AdaptInputFieldSize () {
            var length = MenuUtils.MessagePixelLength(inputField.text, inputField.textComponent);
            var substrings = inputField.text.Split('\n');
            var nWraps = 0;
            for (var i = 0; i< substrings.Length; i++)
            {
                var substringLength = MenuUtils.MessagePixelLength(substrings[i], inputField.textComponent);
                if (substringLength > fieldWidth-50)
                {
                    nWraps += Mathf.CeilToInt(substringLength / (fieldWidth-50));
                }
            }
            nLines = substrings.Length + nWraps;
            if (nLines != oldNlines) {
                oldNlines  = nLines;
                var height = Mathf.Max((nLines-1) * ((inputField.textComponent.GetComponent<UnityEngine.UI.Text>().font.lineHeight + inputField.textComponent.GetComponent<UnityEngine.UI.Text>().fontSize)-9)+44 , 44);
                inputField.gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(fieldWidth, height);
            }
        }
    }
}
