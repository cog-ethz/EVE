using UnityEngine;
using System.Collections;
using System;
using EVE.Scripts.LevelLoader;
using UnityEngine.UI;

public class trainingVideoControl : MonoBehaviour {

    private int currentSetting;
    private const int FORWARD = 0;
    private const int BACKWARD = 1;
    private const int LEFT = 2;
    private const int RIGHT = 3;
    private const int ROTATELEFT = 4;
    private const int ROTATERIGHT = 5;
	private const int LOOKUP = 6;
	private const int LOOKDOWN = 7;
	private const int PRESSTRIGGER = 8;
    private const int PRESSBACK = 9;

    public string[] instructionStrings;

    public GameObject arrows,doorWall;
    public Text instructions;

    private int DONE = 10;

    public UIVideo movDisplay;

    private float oldTransformAngleY;
    private DateTime start;
    private bool changing;
    private LaunchManager launchManager;

    public int changeSeconds;

	// Use this for initialization
	void Start () {
        currentSetting = FORWARD;
        instructions.text = "Move forward";
	    launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        oldTransformAngleY = launchManager.FirstPersonController.transform.localEulerAngles.y;
        //movDisplay.showVideo();
        changing = false;

	}
	
	// Update is called once per frame
	void Update () {
        if (changing)
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > changeSeconds)
            {
                movDisplay.switchToNextMovie();
                changing = false;
                if (currentSetting == DONE)
                {
                    instructions.text = "Move to the next room";
                    movDisplay.hideVideo();
                    openWall();
                }
                else
                {
                    instructions.text = instructionStrings[currentSetting];
                    //movDisplay.playVideo();
                }
                    
            }
            oldTransformAngleY = launchManager.FirstPersonController.transform.localEulerAngles.y;
            
        }
        else
        {
            switch (currentSetting)
            {
                case FORWARD:
                    if (Input.GetAxis("Vertical") > 0)
                    {
                        changing = true;
                        start = DateTime.Now;
                        currentSetting = BACKWARD;
                    }
                    break;
                case BACKWARD:
                    if (Input.GetAxis("Vertical") < 0)
                    {
                        changing = true;
                        start = DateTime.Now;
                        currentSetting = LEFT;
                    }
                    break;
                case LEFT:
                    if (Input.GetAxis("Horizontal") < 0)
                    {
                        changing = true;
                        start = DateTime.Now;
                        currentSetting = RIGHT;
                    }
                    break;
                case RIGHT:
                    if (Input.GetAxis("Horizontal") > 0)
                    {
                        changing = true;
                        start = DateTime.Now;
                        currentSetting = ROTATELEFT;
                    }
                    break;
                case ROTATELEFT:
                    if (isRotating("left"))
                    {
                        changing = true;
                        start = DateTime.Now;
                        currentSetting = ROTATERIGHT;
                    }
                    break;
                case ROTATERIGHT:
                    if (isRotating("right"))
                    {
                        changing = true;
                        start = DateTime.Now;
						currentSetting = LOOKUP;
                    }
                    break;
				case LOOKUP:
					if (Input.GetAxis("Mouse Y") > 0)
					{
						changing = true;
						start = DateTime.Now;
						currentSetting = LOOKDOWN;
					}
					break;
				case LOOKDOWN:
					if (Input.GetAxis("Mouse Y") < 0)
					{
						changing = true;
						start = DateTime.Now;
						currentSetting = PRESSTRIGGER;
					}
					break;
				case PRESSTRIGGER:
					if (Input.GetButtonDown("Back"))
					{
						changing = true;
						start = DateTime.Now;
                        currentSetting = PRESSBACK;
					}
					break;
                case PRESSBACK:
                    if (Input.GetButtonDown("Cancel"))
                    {
                        changing = true;
                        start = DateTime.Now;
                        currentSetting = DONE;
                    }
                    break;
            }
            oldTransformAngleY = launchManager.FirstPersonController.transform.localEulerAngles.y;
        }

      
	}

    bool isRotating(string direction)
    {
        if (direction == "left")
        {
            if (launchManager.FirstPersonController.transform.localEulerAngles.y - oldTransformAngleY < -0.5f)
            {
                oldTransformAngleY = launchManager.FirstPersonController.transform.localEulerAngles.y;
                return true;
            }
            else
                return false;
        }

        else if (direction == "right")
        {
            if (launchManager.FirstPersonController.transform.localEulerAngles.y - oldTransformAngleY > 0.5f)
            {
                oldTransformAngleY = launchManager.FirstPersonController.transform.localEulerAngles.y;
                return true;
            }
            else
                return false;
        }
        else
            return false;

    }

    void openWall()
    {
        doorWall.SetActive(false);
        arrows.SetActive(true);
    }
}
