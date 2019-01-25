using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;

public class MiddleVRLogger : MonoBehaviour {

    public float timestep_in_sec;
    public GameObject player;

    private LoggingManager log;
    private XDocument doc;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        log = launchManager.LoggingManager;

        doc = XDocument.Load(GameObject.FindGameObjectWithTag("MiddleVR").GetComponent<VRManagerScript>().ConfigFile); // load MiddleVR config file (XML)

        foreach (XElement e in doc.Descendants("DisplayManager").DescendantNodes()) // XML Parsing
        {
            if ((e.Name != "Viewport") && (e.Attribute("Tracker").Value != "0"))
            {
                string nodeName = e.Attribute("Name").Value;
                Debug.Log(nodeName);
                GameObject tmp = GameObject.Find(nodeName); // assuming that all MiddleVR nodes have unique names (best design practice)

                string trackerType = e.Attribute("Tracker").Value;
                string useTrackerX = e.Attribute("UseTrackerX").Value;
                string useTrackerY = e.Attribute("UseTrackerY").Value;
                string useTrackerZ = e.Attribute("UseTrackerZ").Value;
                string useTrackerYaw = e.Attribute("UseTrackerYaw").Value;
                string useTrackerPitch = e.Attribute("UseTrackerPitch").Value;
                string useTrackerRoll = e.Attribute("UseTrackerRoll").Value;

                string outputDescription = trackerType + " XYZYPR: " + useTrackerX + useTrackerY + useTrackerZ + useTrackerYaw + useTrackerPitch + useTrackerRoll;
                //Debug.Log(outputDescription);
                StartCoroutine(MVRlogger(nodeName, tmp, outputDescription)); // Minimize operations in coroutine, otherwise it will negatively affect performance! Using Coroutine instead of Invoke because of passing function parameters.
            }
        }
    }

    private void Update()
    {
        if(this.gameObject.transform.parent.gameObject.activeSelf && !player.GetComponentInChildren<ReplayRoute>().isActivated())
        {
            // Keep coroutines running
        }
        else
        {
            StopAllCoroutines(); // Stop all MiddleVR Logging if above conditions no longer hold
        }
    }

    private IEnumerator MVRlogger(string deviceName, GameObject tmp, string description)
    {
        while (true)
        {
            string time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            log.Insert3DMeasurement(deviceName, "Pos " + description, "Meters", tmp.transform.position.x.ToString(), transform.position.y.ToString(), transform.position.z.ToString(), time);
            Debug.Log("Inserted MiddleVR Position Data of " + deviceName);
            log.Insert3DMeasurement(deviceName, "Rot " + description, "Degrees", tmp.transform.rotation.eulerAngles.x.ToString(), tmp.transform.rotation.eulerAngles.y.ToString(), tmp.transform.rotation.eulerAngles.z.ToString(), time);
            Debug.Log("Inserted MiddleVR Rotation Data of " + deviceName);
            yield return new WaitForSeconds(timestep_in_sec);
        }
    }

}