using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptInputFieldSize : MonoBehaviour {

    public UnityEngine.UI.InputField inputField;
    private int nLines = 0, oldNlines = 0;

    private int fieldWidth;

    void Start()
    {
        fieldWidth = (int) inputField.GetComponent<RectTransform>().sizeDelta.x;
    }

	// Update is called once per frame
	void Update () {
        int length = CalculateLengthOfMessage(inputField.text, inputField.textComponent);
        string[] substrings = inputField.text.Split('\n');
        int nWraps = 0;
        for (int i = 0; i< substrings.Length; i++)
        {
            int substringLength = CalculateLengthOfMessage(substrings[i], inputField.textComponent);
            if (substringLength > fieldWidth-50)
            {
                nWraps += Mathf.CeilToInt(substringLength / (fieldWidth-50));
            }
        }
        nLines = substrings.Length + nWraps;
        if (nLines != oldNlines) {
            oldNlines  = nLines;
            int height = Mathf.Max((nLines-1) * ((inputField.textComponent.GetComponent<UnityEngine.UI.Text>().font.lineHeight + inputField.textComponent.GetComponent<UnityEngine.UI.Text>().fontSize)-9)+44 , 44);
            inputField.gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(fieldWidth, height);
        }
	}

    private int CalculateLengthOfMessage(string message, UnityEngine.UI.Text tex)
    {
        int totalLength = 0;

        Font myFont = tex.font;
        CharacterInfo characterInfo = new CharacterInfo();

        char[] arr = message.ToCharArray();

        foreach (char c in arr)
        {
            myFont.GetCharacterInfo(c, out characterInfo, tex.fontSize);

            totalLength += characterInfo.advance;
        }

        return totalLength;
    }
}
