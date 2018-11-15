using UnityEngine;
using System.Threading;
using System;

public class BPDelayCurtain : MonoBehaviour
{
    [Tooltip("Objects that are set as overlay and need to be deactivated manually")]
    public GameObject[] overlayObjects;
    private TimeSpan diff, intervall;
    private DateTime timeNow;
    private int mod;
    private LaunchManager launchManager;
    private Camera fpcCamera, delayCamera;
    private HL7ServerStarter srv;
    //private bool fpcActive;

    // Use this for initialization
    void Awake()
    {
        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        srv = launchManager.gameObject.GetComponent<HL7ServerStarter>();
        if (srv.isActiveAndEnabled && !launchManager.FPC.GetComponentInChildren<ReplayRoute>().isActivated())
        {
            timeNow = DateTime.Now;
            intervall = srv.getIntervall();
            mod = (timeNow.Minute % intervall.Minutes);
            diff = intervall - new TimeSpan(0, (int)mod, timeNow.Second);
            if (diff.TotalSeconds < 30)
                diff = diff.Add(intervall);
            fpcCamera = launchManager.FPC.GetComponentInChildren<Camera>();
            delayCamera = GetComponent<Camera>();
            foreach (var obj in overlayObjects)
            {
                obj.SetActive(false);
            }
           // fpcActive = launchManager.FPC.gameObject.activeSelf;
            //launchManager.FPC.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
       
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.Home)
        {
            diff = diff.Subtract(diff);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!launchManager.FPC.GetComponentInChildren<ReplayRoute>().isActivated())
        {
            // Wait for the next blood pressure measurement to start together with the level
            if (diff.TotalSeconds > 1)
            {                
                Time.timeScale = 0;
                fpcCamera.enabled = false;
                delayCamera.enabled = true;
                TimeSpan tmp = DateTime.Now.Subtract(timeNow);
                diff = diff.Subtract(tmp);
                timeNow = DateTime.Now;
                mod = (timeNow.Minute % intervall.Minutes);
            }
            else
            {

               // launchManager.FPC.gameObject.SetActive(fpcActive);
                foreach (var obj in overlayObjects)
                {
                    obj.SetActive(true);
                }

                Time.timeScale = 1;
                delayCamera.enabled = false;
                fpcCamera.enabled = true;
                gameObject.SetActive(false);
            }
        }
    }
        
}