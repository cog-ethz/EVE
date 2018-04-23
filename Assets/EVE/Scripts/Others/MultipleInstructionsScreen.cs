using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class MultipleInstructionsScreen : MonoBehaviour
{

    //style
    public GUISkin skin;

    public int windowWidth = 1400;
    public int windowHeight = 900;

    public string[] text;
    public string[] underImage;
    public Texture2D[] InstImage;

    public int[] imageSizeX, imageSizeY, imagePosYDiff;

    private int currentScreen;
    private bool pressedBttn, pressedCancelBttn;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < text.Length; i++)
        {
            text[i] = text[i].Replace("NEWLINE", "\n");
        }
        currentScreen = 0;
    }


    void OnGUI()
    {

        GUILayout.BeginArea(new Rect((Screen.width - windowWidth) / 2, (Screen.height - windowHeight) / 2, windowWidth, windowHeight));
        if (currentScreen < text.Length)
            GUILayout.Label(text[currentScreen], skin.label);

        if (currentScreen < InstImage.Length)
            if (InstImage[currentScreen] != null)
                GUI.DrawTexture(new Rect((windowWidth / 2) - imageSizeY[currentScreen] / 2, (windowHeight / 2) - imagePosYDiff[currentScreen], imageSizeY[currentScreen], imageSizeX[currentScreen]), InstImage[currentScreen]);
        if (currentScreen < underImage.Length)
            GUI.Label(new Rect(0, (windowHeight - 200), windowWidth, 150), underImage[currentScreen], skin.label);

        // Buttons
        if (currentScreen >= 0 && (currentScreen < text.Length || currentScreen < underImage.Length || currentScreen < InstImage.Length))
        {
            if (GUI.Button(new Rect(windowWidth / 2 + 50, windowHeight - 100, 200, 50), "Continue", skin.button) | pressedBttn)
            {
                pressedBttn = false;
                if (currentScreen == Math.Max(Math.Max(text.Length, underImage.Length), InstImage.Length) - 1)
                {
                    SceneManager.LoadScene("Loader");
                }
                else
                    currentScreen++;
            }
        }
        if (currentScreen > 0)
        {
            if (GUI.Button(new Rect(windowWidth / 2 - 250, windowHeight - 100, 200, 50), "Back", skin.button) | pressedCancelBttn)
            {
                pressedCancelBttn = false;
                currentScreen--;
            }
        }

        GUILayout.EndArea();
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Loader");
        }
    }

    // Update is called once per frame
    void Update()
    {
        pressedBttn = false;
        pressedCancelBttn = false;
        if (Input.GetButtonDown("Back"))
        {
            pressedBttn = true;
        }
        else if (Input.GetButtonDown("Cancel"))
        {
            pressedCancelBttn = true;
        }
    }


}
