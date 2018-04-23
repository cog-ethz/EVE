using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class WaitScreen : MonoBehaviour {

    //style
    public GUISkin skin;
    public GUISkin leftAllignedSkin;

    public int waitTime = 0;

    public int windowWidth = 1400;
    public int windowHeight = 900;

    public bool skipWithButtons = true;

    private DateTime start;

    private bool pressedOnce = false, pressedBttn, bttnreleased, loading = false; 

	// Use this for initialization
	void Start () {
        start = DateTime.Now;
        pressedOnce = false;
        pressedBttn = false;
        bttnreleased = false;
	}


    void OnGUI()
    {
        if (skipWithButtons)
        {
            GUILayout.BeginArea(new Rect((Screen.width - windowWidth) / 2, (Screen.height - windowHeight) / 2, windowWidth, windowHeight));

            if (DateTime.Now.Subtract(start).TotalSeconds > waitTime)
            {
                if (!pressedOnce)
                {
                    GUI.Label(new Rect(windowWidth / 2 - 250, windowHeight - 200, 500, 500), "Press when you are ready to start", skin.label);

                    // Buttons
                    if (GUI.Button(new Rect(windowWidth / 2 - 100, windowHeight - 100, 200, 50), "Continue", skin.button) | pressedBttn)
                    {
                        pressedOnce = true;
                        pressedBttn = false;
                    }
                }
                else
                {
                    GUI.Label(new Rect((windowWidth / 2) - 250, windowHeight - 200, 500, 500), "Press again to confirm", skin.label);

                    // button to get back to the level loader
                    if (GUI.Button(new Rect(windowWidth / 2 - 100, windowHeight - 100, 200, 50), "Start", skin.button) | pressedBttn)
                    {
                        pressedBttn = false;
                        bttnreleased = true;
                        SceneManager.LoadScene("Loader");

                    }
                }


            }


            GUILayout.EndArea();
        }
        else
        {
             if (DateTime.Now.Subtract(start).TotalSeconds > waitTime)
                 if (!loading)
                 {
                     loading = true;
                    SceneManager.LoadScene("Loader");
                 }
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Loader");
        }
    }
	
	// Update is called once per frame
	void Update () {
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
