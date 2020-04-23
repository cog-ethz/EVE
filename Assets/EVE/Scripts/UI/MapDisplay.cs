using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;
using EVE.Scripts.LevelLoader;
using UnityEngine.Networking;

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
    private double screenPixelX, screenPixelY;

    public int ResolutionWidth = 1920, ResolutionHeight = 1080;
    private LaunchManager launchManager;

    private Matrix4x4 projectionMatrices, worldToCameraMatrices;


    // Use this for initialization
    void Start()
    {
        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        log = launchManager.LoggingManager;

        rpl = launchManager.FirstPersonController.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
        if (rpl.isActivated())
        {
            var parameterValue = 0;
            int.TryParse(launchManager.LoggingManager.GetParameterValue(launchManager.ReplaySessionId,"mapType"), out parameterValue);
            mapType = parameterValue;
        }
        else
        {
            var parameterValue = 0;
            int.TryParse(launchManager.LoggingManager.GetParameterValue("mapType"), out parameterValue);
            mapType = parameterValue;
        }                     

        whiteBG = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        whiteBG.SetPixel(0, 0, Color.white);
        whiteBG.Apply();
        
        SetupMap(SceneManager.GetActiveScene().name);
     
        posX = Screen.width - mapWidth;
        posY = 0;//Screen.width - mapWidth;

        mainCam = launchManager.FirstPersonController.GetComponentInChildren<Camera>();
        arrow = launchManager.FirstPersonController.transform.Find("GuidanceArrow").transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (mapTexture != null)
        {
            mapHeight = Screen.height* mapScale;
            mapWidth = (mapHeight / mapTexture.height) * mapTexture.width;
            /*mapWidth = Screen.width * 1.0f;
            mapHeight = (mapWidth / mapTexture.width) * mapTexture.height;*/

            float top = ((float)Screen.height - mapHeight) / 2f;
            float left = ((float)Screen.width - mapWidth) / 2f;

            Vector3 worldLocation = new Vector3(launchManager.FirstPersonController.transform.position.x, launchManager.FirstPersonController.transform.position.y, launchManager.FirstPersonController.transform.position.z);

           // Camera c = GameObject.Find("EvaluationCamera").GetComponent<Camera>();
          
            Vector3 screenPoint = projectionMatrices.MultiplyPoint(worldToCameraMatrices.MultiplyPoint(worldLocation));


            //screenPixel = new Vector2(left + (screenPoint[0] + 0.5f) * mapWidth, top + (-screenPoint[1] + 1.0f) * 0.5f * mapHeight);
            screenPixel = new Vector2(left + ((screenPoint[0] + 1.0f)*0.5f* mapWidth), (top +(-screenPoint[1] + 1.0f)*0.5f * mapHeight));

            if (goal != null)
            {
                screenPoint = projectionMatrices.MultiplyPoint(worldToCameraMatrices.MultiplyPoint(goal.transform.position));
                goalScreenPixel = new Vector2(Screen.width - screenPoint[0], screenPoint[1]);
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
                        if (!rpl.isActivated()) log.InsertLiveMeasurement("Map", "Map showed", "sec", DateTime.Now.Subtract(pressed).TotalSeconds.ToString());
                    }

                    showMap = false;
                }


            }
        }
        else
        {
            SetupMap(SceneManager.GetActiveScene().name);
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
                   // GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteBG);
                    GUI.color = new Color(1.0f, 1.0f, 1.0f, opacity);
                    GUI.DrawTexture(new Rect(posX, posY, mapWidth, mapHeight), mapTexture);                    

                   if (goal != null)
                    {
                        var goalMarkerPos = new Rect(goalScreenPixel[0] - (bmwidth / 2), goalScreenPixel[1] - (bmheight / 2), bmwidth, bmheight);
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
                    GUI.DrawTexture(mapMarkerPos, playerMarker);                    
                    GUI.matrix = matrixBackup;
                    break;
                case 1:
                   /* GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteBG);
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

                    break;*/
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
            if (mapType == 2 && !rpl.isActivated())
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

    private IEnumerator LoadMap(string mapName, int width, int height)
    {
        var path = "file:///" + Application.persistentDataPath + "/maps/" + mapName + "_" + width + "x" + height + ".png";
        
        var uwr = new UnityWebRequest(path);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            mapTexture = DownloadHandlerTexture.GetContent(uwr);
        }
    }

    /// <summary>
    /// Loads map from the persistent data path.
    /// </summary>
    /// <remarks>
    /// The maps need to be generated by placing an `EvaluationCamera`-Prefab in the scene.
    /// </remarks>
    /// <param name="envName"Map names</param>
    internal void SetupMap(string envName)
    {
        projectionMatrices = new Matrix4x4();
        worldToCameraMatrices = new Matrix4x4();

        try
        {
            string path = Application.persistentDataPath + "/maps/" + envName + "_worldToCameraMatrix.xml";

            XmlSerializer xmls = new XmlSerializer(new Matrix4x4().GetType());
            using (var stream = File.OpenRead(path))
            {
                worldToCameraMatrices = (Matrix4x4)xmls.Deserialize(stream);
            }
            path = Application.persistentDataPath + "/maps/" + envName + "_projectionMatrix.xml";
            using (var stream = File.OpenRead(path))
            {
                projectionMatrices = (Matrix4x4)xmls.Deserialize(stream);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("A matrix for " + envName + " was not found:\n" + ex.StackTrace);
        }
        mapTexture = new Texture2D(2,2);

        StartCoroutine(LoadMap(envName, ResolutionWidth, ResolutionHeight));
    }

    // -----------------------------------------
    //			 mouse motion enable/disable
    //------------------------------------------
    public void StopAllMotion()
    {
        //disable movement
        if (!launchManager.FirstPersonController.GetComponentInChildren<ReplayRoute>().isActivated())
        {
            launchManager.FirstPersonController.GetComponent<CharacterController>().enabled = false;
            launchManager.FirstPersonController.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
        }
        
    }

    public void ContinueMotion()
    {
        //enable movement
        if (!launchManager.FirstPersonController.GetComponentInChildren<ReplayRoute>().isActivated())
        {
            launchManager.FirstPersonController.GetComponent<CharacterController>().enabled = true;
            launchManager.FirstPersonController.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
        }
            
    }
}
