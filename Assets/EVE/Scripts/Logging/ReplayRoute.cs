using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using EVE.Scripts.LevelLoader;
using EVE.Scripts.Utils;
using UnityEngine.SceneManagement;

public class ReplayRoute : MonoBehaviour {

	public GameObject player;
	public LogPosition pos_logger;

    private LineRenderer playbackLineRenderer;
    private float timeSpent;
	private LoggingManager log;
	private List<float>[] xyz;
	private List<string>[] _input;
	//private int input_pointer = 0; //TODO Implement input replay
	private int pos_pointer = 0, start_pointer;
	private string[] sceneTime;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController movementControls;

    private float currentSliderValue = 0f, lastSliderValue;
    private string playbackMode = "", playbackCamera = "";
    private float time;
    private float[] timeDifferences;
    private DateTime playStart;
    private bool activated;
    private LaunchManager launchManager;
    private GameObject evalCamera;



    private float pos_x = 0, pos_y = 0, pos_z = 0, view_x = 0, view_y = 0, view_z = 0;
    private int xyzCount;

    // Use this for initialization
	void Start () {
        playbackCamera = "FirstPersonCharacter";
	    launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
	    evalCamera = GameObject.Find("EvaluationCamera");
	}

    public void activateReplay(int sessionID, string sceneName, int sceneID)
    {
        if (launchManager == null)
        {
            launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            evalCamera = GameObject.Find("EvaluationCamera");
        }
        
        launchManager.MenuCanvas.SetActive(false);
        launchManager.ReplaySessionId = sessionID;
        log = launchManager.LoggingManager;
        pos_logger.enabled = false;
        xyz = log.GetPath(sessionID, sceneID);
        xyzCount = xyz[0].Count - 1;
        _input = log.GetAllInput(sessionID, sceneID);
        sceneTime = log.GetSceneTime(sceneName, sessionID);

        movementControls = player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        movementControls.enabled = false;
        activated = true;
    }

    void OnGUI()
    {
        if (activated)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            currentSliderValue = GUI.HorizontalSlider(new Rect(130, 20, 300, 30), currentSliderValue, 0, xyzCount);
            GUI.Label(new Rect(440, 15, 100, 30), (int)currentSliderValue + "/" + xyzCount);
            if (playbackMode == "playing")
            {
                if (GUI.Button(new Rect(5, 10, 120, 30), "Stop"))
                {
                    StopPlayback();
                }
            }
            else
            {
                if (GUI.Button(new Rect(5, 10, 120, 30), "Play"))
                {
                    if (currentSliderValue > 0)
                    {
                        // If we have scrubbed in the timeline, playback from that position
                        pos_pointer = (int)currentSliderValue;

                    }
                    else
                    {
                        // otherwise start at the first frame.
                        pos_pointer = 0;
                    }
                    playbackMode = "playing";
                    playStart = DateTime.Now;
                    start_pointer = pos_pointer;
                }
                else if (GUI.Button(new Rect(5, 95, 120, 30), "Exit"))
                {
                    // remove LineRenderer
                    Destroy(launchManager.FirstPersonController.GetComponent<LineRenderer>());
                    playbackCamera = "FirstPersonCharacter";
                    //evalCamera.GetComponent<Camera>().enabled = false;
                    launchManager.FirstPersonController.GetComponentInChildren<Camera>().enabled = true;
                    
                    launchManager.MenuCanvas.SetActive(true);
                    SceneManager.LoadScene("Launcher");
                }
            }
            if (playbackCamera == "FirstPersonCharacter")
            {
                if (GUI.Button(new Rect(5, 50, 120, 30), "Bird's Eye"))
                {
                    playbackLineRenderer = launchManager.FirstPersonController.AddComponent(typeof(LineRenderer)) as LineRenderer;
                    playbackLineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                    var alpha = 1.0f;
                    var gradient = new Gradient();
                    gradient.SetKeys(
                        new[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                        new[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                        );
                    playbackLineRenderer.colorGradient = gradient;

                    var curve = new AnimationCurve();                 
                    curve.AddKey(0.0f, 1.0f);
                    curve.AddKey(1.0f, 1.0f);

                    playbackLineRenderer.widthCurve = curve;
                    playbackLineRenderer.widthMultiplier = 1f;

                    // playbackLineRenderer.SetColors(Color.red, Color.red);
                    //playbackLineRenderer.SetWidth(0.2f, 0.2f);

                    playbackCamera = "EvaluationCamera";                    
                    launchManager.FirstPersonController.GetComponentInChildren<Camera>().enabled = false;
                    evalCamera.GetComponent<Camera>().enabled = true;
                }
            }
            else if (playbackCamera == "EvaluationCamera")
            {
                if (GUI.Button(new Rect(5, 50, 120, 30), "First Person"))
                {
                    // remove LineRenderer
                    Destroy(launchManager.FirstPersonController.GetComponent<LineRenderer>());

                    playbackCamera = "FirstPersonCharacter";
                    evalCamera.GetComponent<Camera>().enabled = false;
                    launchManager.FirstPersonController.GetComponentInChildren<Camera>().enabled = true;
                }
            }
        }
        
    }
	
	// Update is called once per frame
    void Update()
    {
        if (activated)
        {
            if ((playbackMode == "playing"))
            {
                if (currentSliderValue != lastSliderValue)
                {
                    lastSliderValue = currentSliderValue;
                    playStart = DateTime.Now;
                    start_pointer = (int)currentSliderValue;
                }

                pos_pointer = (int)currentSliderValue;
                // replay position and view
                if (pos_pointer < xyzCount - 1)
                {
                    timeSpent = (float)DateTime.Now.Subtract(playStart).TotalSeconds + xyz[6][start_pointer];
                    var nextPosTime = xyz[6][pos_pointer + 1];  // in seconds
                    var t = timeSpent - nextPosTime;
                    float p = 0;

                    if (t >= 0)
                    {
                        currentSliderValue++;
                        pos_pointer++;
                        // pos_pointer was increased by one therefore pos_pointer + 1 is now one step more!
                        var nextNextPosTime = xyz[6][pos_pointer + 1];  // in seconds
                        p = pos_pointer + (timeSpent - nextPosTime) / (nextNextPosTime - nextPosTime);
                    }
                    else
                    {
                        var oldPosTime = xyz[6][pos_pointer];   // in seconds
                        p = pos_pointer + (timeSpent - oldPosTime) / (nextPosTime - oldPosTime);
                    }
                    
                    if (p >= 0 && p < xyzCount)
                    {
                        pos_x = xyz[0][(int)Mathf.Floor(p)] + (xyz[0][(int)Mathf.Ceil(p)] - xyz[0][(int)Mathf.Floor(p)]) * (p % 1);
                        pos_y = xyz[1][(int)Mathf.Floor(p)] + (xyz[1][(int)Mathf.Ceil(p)] - xyz[1][(int)Mathf.Floor(p)]) * (p % 1);
                        pos_z = xyz[2][(int)Mathf.Floor(p)] + (xyz[2][(int)Mathf.Ceil(p)] - xyz[2][(int)Mathf.Floor(p)]) * (p % 1);
                        view_x = xyz[3][(int)Mathf.Floor(p)] + (xyz[3][(int)Mathf.Ceil(p)] - xyz[3][(int)Mathf.Floor(p)]) * (p % 1);
                        view_y = xyz[4][(int)Mathf.Floor(p)] + (xyz[4][(int)Mathf.Ceil(p)] - xyz[4][(int)Mathf.Floor(p)]) * (p % 1);
                        view_z = xyz[5][(int)Mathf.Floor(p)] + (xyz[5][(int)Mathf.Ceil(p)] - xyz[5][(int)Mathf.Floor(p)]) * (p % 1);
                    }
                }

                player.transform.position = new Vector3(pos_x, pos_y, pos_z);
                player.transform.eulerAngles = new Vector3(view_x, view_y, view_z);

                if (playbackLineRenderer == null) return;
                playbackLineRenderer.positionCount = pos_pointer;
                var i = 0;
                while (i < pos_pointer)
                {
                    playbackLineRenderer.SetPosition(i, new Vector3(xyz[0][i], xyz[1][i], xyz[2][i]));
                    i++;
                }
            }
        }
       
    }

    private void StopPlayback() {
        playbackMode = "";
        pos_pointer = 0;
    }

    public bool isActivated()
    {
        return activated;
    }

    public float getTimeSpent()
    {
        return timeSpent;
    }
}
