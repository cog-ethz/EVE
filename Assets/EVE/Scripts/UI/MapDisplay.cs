using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class MapDisplay : MonoBehaviour
{

    public Texture playerMarker, goalMarker;   
    public float mapScale;
    public float markerScale;
    public float opacity;

    public Transform goal;

    private float mapHeight;
    private float mapWidth;

    private float angle;
    private float posX;
    private float posY;

    private bool showMap, pressedLastFrame;
    private DateTime released, pressed;
    private float blockSeconds = 0f;
    private LoggingManager log;
    private Texture2D whiteBG;
    private Transform arrow;
    private Camera mainCam;

    private ReplayRoute rpl;
    private int mapType;
      
    //map data
    private Texture mapTexture;
    private Vector2 screenPixel, goalScreenPixel;

    private int resolution = 2048;
    private LaunchManager launchManager;


    // Use this for initialization
    void Start()
    {
        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        log = launchManager.GetLoggingManager();

        rpl = launchManager.FPC.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
        if (rpl.isActivated())
        {
            mapType = int.Parse(launchManager.GetLoggingManager().getParameterValue(launchManager.getReplaySessionId(),"mapType"));
        }
        else
        {
            mapType = int.Parse(launchManager.GetLoggingManager().getParameterValue("mapType"));
        }                     

        whiteBG = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        whiteBG.SetPixel(0, 0, Color.white);
        whiteBG.Apply();
        
        SetupMap(SceneManager.GetActiveScene().name);
     
        posX = Screen.width - mapWidth;
        posY = 0;//Screen.width - mapWidth;

        mainCam = GameObject.Find("FirstPersonCharacter").GetComponent<Camera>();
        arrow = launchManager.FPC.transform.Find("GuidanceArrow").transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (mapTexture != null)
        {
            mapWidth = Screen.width * mapScale;
            mapHeight = (mapWidth / mapTexture.width) * mapTexture.height;

            Vector3 worldLocation = new Vector3(GameObject.Find("FPSController").transform.position.x, GameObject.Find("FPSController").transform.position.y, GameObject.Find("FPSController").transform.position.z);

            Camera c = GameObject.Find("EvaluationCamera").GetComponent<Camera>();
            Vector3 tmp = c.WorldToScreenPoint(worldLocation);

            screenPixel = new Vector2(Screen.width - tmp[0], tmp[1]);
            
            if (goal != null)
            {
                tmp = c.WorldToScreenPoint(goal.transform.position);
                goalScreenPixel = new Vector2(Screen.width - tmp[0], tmp[1]);
            }

            angle = GameObject.Find("FPSController").transform.eulerAngles.y;

            posX = ((Screen.width - mapWidth) / 2);
            posY = (Screen.height - mapHeight) / 2;
            // || (parameters.input == "confirm" && parameters.fake_input)) && (DateTime.Now.Subtract(released).TotalSeconds > blockSeconds 
            if (Input.GetButton("Back") && (DateTime.Now.Subtract(released).TotalSeconds > blockSeconds || DateTime.Now.Subtract(released).TotalSeconds < 0.2f))
            {
                if (!showMap)
                    pressed = DateTime.Now;
                showMap = true;
                if (!rpl.isActivated()) log.LogInput("confirm");
                pressedLastFrame = true;
            }
            else
            {
                if (pressedLastFrame)
                {
                    released = DateTime.Now;
                    pressedLastFrame = false;
                }
                if (DateTime.Now.Subtract(released).TotalSeconds > 0.2f)
                {
                    if (showMap)
                    {
                        if (!rpl.isActivated()) log.insertLiveMeasurement("Map", "Map showed", "sec", DateTime.Now.Subtract(pressed).TotalSeconds.ToString());
                    }

                    showMap = false;
                }


            }
        }
       
    }


    public void OnGUI()
    {
        if (showMap)
        {
            StopAllMotion();            
            float bmwidth = playerMarker.width * markerScale;
            float bmheight = playerMarker.height * markerScale;
            switch (mapType)
            {
                case 0:
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteBG);
                    GUI.color = new Color(1.0f, 1.0f, 1.0f, opacity);
                    GUI.DrawTexture(new Rect(posX, posY, mapWidth, mapHeight), mapTexture);

                    if (goal != null)
                    {
                        Rect goalMarkerPos = new Rect(goalScreenPixel[0] - (bmwidth / 2), goalScreenPixel[1] - (bmheight / 2), bmwidth, bmheight);
                        Matrix4x4 matrixBackup2 = GUI.matrix;
                        Vector2 pivot2 = new Vector2(Screen.width / 2f, Screen.height / 2f);
                        GUIUtility.RotateAroundPivot(GameObject.Find("EvaluationCamera").transform.eulerAngles.y, pivot2);
                        GUI.DrawTexture(goalMarkerPos, goalMarker);
                        GUI.matrix = matrixBackup2;
                    }

                    Rect mapMarkerPos = new Rect(screenPixel[0] - (bmwidth / 2), screenPixel[1] - (bmheight / 2), bmwidth, bmheight);
                    Matrix4x4 matrixBackup = GUI.matrix;
                    Vector2 pivot = new Vector2(mapMarkerPos.xMin + mapMarkerPos.width * 0.5f, mapMarkerPos.yMin + mapMarkerPos.height * 0.5f);
                    GUIUtility.RotateAroundPivot(angle, pivot);
                    pivot = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    GUIUtility.RotateAroundPivot(GameObject.Find("EvaluationCamera").transform.eulerAngles.y, pivot);
                    GUI.DrawTexture(mapMarkerPos, playerMarker);
                    
                    GUI.matrix = matrixBackup;
                    break;
                case 1:
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteBG);
                    GUI.color = new Color(1.0f, 1.0f, 1.0f, opacity);
                                                      
                    Vector3 newPoint = RotatePointAroundPivot(new Vector3(screenPixel[0], screenPixel[1], 0), new Vector3((Screen.width / 2), (Screen.height / 2) , 0), new Vector3(0, 0, (360- angle)/2));
                    screenPixel[0] = newPoint.x;
                    screenPixel[1] = newPoint.y;
                    mapMarkerPos = new Rect(screenPixel[0] - (bmwidth / 2), screenPixel[1] - (bmheight / 2), bmwidth, bmheight);                   

                    matrixBackup = GUI.matrix;
                    pivot = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    GUIUtility.RotateAroundPivot((360 -  angle), pivot);
                    GUIUtility.RotateAroundPivot(GameObject.Find("EvaluationCamera").transform.eulerAngles.y, pivot);
                    GUI.DrawTexture(new Rect(posX, posY, mapWidth, mapHeight), mapTexture);
                    GUI.matrix = matrixBackup;

                    GUI.DrawTexture(mapMarkerPos, playerMarker);

                    if (goal != null)
                    {
                        Vector3 newGoalPoint = RotatePointAroundPivot(new Vector3(goalScreenPixel[0], goalScreenPixel[1], 0), new Vector3((Screen.width / 2), (Screen.height / 2), 0), new Vector3(0, 0, (360 - angle) / 2));
                        goalScreenPixel[0] = newGoalPoint.x;
                        goalScreenPixel[1] = newGoalPoint.y;
                        Rect goalMarkerPos = new Rect(goalScreenPixel[0] - (bmwidth / 2), goalScreenPixel[1] - (bmheight / 2), bmwidth, bmheight);
                        GUI.DrawTexture(goalMarkerPos, goalMarker);
                    }

                    break;
                case 2:

                    arrow.gameObject.SetActive(true);
                    arrow.transform.Find("ArrowAndWalls").transform.Find("ArrowParent").GetComponent<GuidanceArrow>().setTarget(goal);
                    mainCam.enabled = false;
                    break;
            }



        }
        else
        {
           arrow.gameObject.SetActive(false);
           mainCam.enabled = true;
           ContinueMotion();
        }

    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    internal void SetupMap(string envName)
    {        
        mapTexture = new Texture();
        
        StartCoroutine(LoadMap(envName, resolution));        

    }

    private IEnumerator LoadMap(string mapName, int resolution)
    {
        string path = "file:///" + Application.persistentDataPath + "/maps/" + mapName + "_" + resolution + "x" + resolution + ".png";

        WWW www = new WWW(path);
        yield return www;
        Texture2D tmpTex = new Texture2D(2, 2);
        www.LoadImageIntoTexture(tmpTex);
        mapTexture = tmpTex;
    }

    // -----------------------------------------
    //			 mouse motion enable/disable
    //------------------------------------------
    public void StopAllMotion()
    {
        //disable movement
        launchManager.FPC.GetComponent<CharacterController>().enabled = false;
        launchManager.FPC.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
    }

    public void ContinueMotion()
    {
        //enable movement
        launchManager.FPC.GetComponent<CharacterController>().enabled = true;
        launchManager.FPC.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
    }
}
