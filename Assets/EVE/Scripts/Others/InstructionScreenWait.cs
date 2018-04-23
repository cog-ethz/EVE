using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class InstructionScreenWait : MonoBehaviour {

    //style
    public GUISkin skin;
    public GUISkin leftAllignedSkin;

    public int waitTime = 0;

    public int windowWidth = 1400;
    public int windowHeight = 900;

    public bool showButtons;

    public string text = "";
    public string underImage = "";
    public Texture2D InstImage;

    public int imageX = 691, imageY = 328, ImageHeightDiff = 75;

    private DateTime start;

    private bool pressedOnce = false, pressedBttn, bttnreleased, loading = false;

    // Use this for initialization
    void Start()
    {
        start = DateTime.Now;
        text = text.Replace("NEWLINE", "\n");
    }


    void OnGUI()
    {
 
        GUILayout.BeginArea(new Rect((Screen.width - windowWidth) / 2, (Screen.height - windowHeight) / 2, windowWidth, windowHeight));
        GUILayout.Label(text, skin.label);
        //GUILayout.Label("This is a the instruction screen for both the learning phase and the testing phase. Please read carefully! Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.", skin.label);
        //GUILayout.Label("", skin.label);

        if (showButtons)
        {
            if (InstImage != null)
                GUI.DrawTexture(new Rect((windowWidth / 2) - imageX / 2, (windowHeight / 2) - ImageHeightDiff, imageX, imageY), InstImage);
            GUI.Label(new Rect(0, (windowHeight -200), windowWidth, 150), underImage, skin.label);

            if (DateTime.Now.Subtract(start).TotalSeconds > waitTime)
            {
                if (!pressedOnce)
                {
                    GUI.Label(new Rect((windowWidth / 2) - 250, windowHeight-50, 550, 500), "Press when you are ready to continue", skin.label);

                    // Buttons
                    if (GUI.Button(new Rect(windowWidth / 2 - 100, windowHeight - 100, 200, 50), "Continue", skin.button) | pressedBttn)
                    {
                        pressedOnce = true;
                        pressedBttn = false;
                    }
                }
                else
                {
                    GUI.Label(new Rect((windowWidth / 2) - 170, windowHeight - 50, 500, 500), "Press again to confirm", skin.label);

                    // button to get back to the level loader
                    if (GUI.Button(new Rect(windowWidth / 2 - 100, windowHeight - 100, 200, 50), "Confirm", skin.button) | pressedBttn)
                    {
                        pressedBttn = false;
                        bttnreleased = true;
                        SceneManager.LoadScene("Loader");

                    }
                }


            }
        }
        else
        {
            if (InstImage != null)
                GUI.DrawTexture(new Rect((windowWidth / 2) - imageX / 2, (windowHeight / 2) - ImageHeightDiff, imageX, imageY), InstImage);
            GUI.Label(new Rect(0, (windowHeight - 200), windowWidth, 150), underImage, skin.label);

            if (DateTime.Now.Subtract(start).TotalSeconds > waitTime)
            {
                if (!loading)
                {
                    loading = true;
                    SceneManager.LoadScene("Loader");
                }

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
        if (showButtons)
        {
            pressedBttn = false;
            if ((!pressedOnce && Input.GetButtonDown("Back")) | (bttnreleased && Input.GetButtonDown("Back")))
            {
                pressedBttn = true;
            }
            if (pressedOnce)
            {
                if (Input.GetButtonUp("Back"))
                {
                    bttnreleased = true;
                }
            }
        }
    }
}
