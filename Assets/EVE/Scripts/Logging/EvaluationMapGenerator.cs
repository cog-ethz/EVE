using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.Xml;

public class EvaluationMapGenerator : MonoBehaviour
{

    [Header("Map Properties")]
    [Tooltip("Width of the map image")]
    public int width;
    [Tooltip("Height of the map image")]
    public int height;
    [Tooltip("Force creation of map image")]
    public bool forceCreate;
    private string filePathRoot;
    private string sceneName;

    public Camera Camera;

    // Use this for 
    void Start()
    {

        filePathRoot = Application.persistentDataPath + "/maps/";
        sceneName = SceneManager.GetActiveScene().name;
        DirectoryInfo dirInf = new DirectoryInfo(filePathRoot);
        if (!dirInf.Exists)
        {
            UnityEngine.Debug.Log("Creating subdirectory for maps"); dirInf.Create();
            UnityEngine.Debug.Log("Maps stored at " + filePathRoot);
            dirInf.Create();
        }
        string screenShotName = ScreenShotName();
        bool computeMapImage = forceCreate;
        if (!forceCreate)
        {
            FileInfo[] fileInfo = dirInf.GetFiles();
            foreach (FileInfo file in fileInfo)
            {
                if (file.Name.Equals(screenShotName))
                {
                    computeMapImage = false;
                }
            }
        }
        if (computeMapImage)
        {
            Camera.enabled = true;
            Camera.aspect = width / height;
            //Camera camera = GetComponent<Camera>();
            Camera.pixelRect = new Rect(0, 0, width, height);
            RenderTexture rt = new RenderTexture(width, height, 24);
            Camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            Camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            Camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(screenShotName, bytes);
            Debug.Log(string.Format("Created a map for: {0}", screenShotName));
            //camera.worldToCameraMatrix

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
            
            XmlSerializer xmls = new XmlSerializer(Camera.worldToCameraMatrix.GetType());
            using (var stream = File.Open(filePathRoot + sceneName + "_worldToCameraMatrix.xml", FileMode.Create))
            {
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    xmls.Serialize(xmlWriter, Camera.worldToCameraMatrix, ns);
                }
            }

            using (var stream = File.Open(filePathRoot + sceneName + "_projectionMatrix.xml", FileMode.Create))
            {
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    xmls.Serialize(xmlWriter, Camera.projectionMatrix, ns);
                }
            }
            Camera.ResetAspect();
            Camera.enabled = false;
        }
        Destroy(this);
    }

    /// <summary>
    /// Creates a string tp
    /// </summary>
    /// <returns></returns>
    private string ScreenShotName()
    {
        return string.Format("{0}{1}_{2}x{3}.png",
                             filePathRoot,
                             sceneName,
                             width, height);
    }
}