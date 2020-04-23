using UnityEngine;
using System.Collections;
using EVE.Scripts.LevelLoader;

public class EventMarker: MonoBehaviour {

	public string eventName;
	private LoggingManager _log;
    private LaunchManager launchManager;



    void Start() {
        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
	    _log = launchManager.LoggingManager;
	}
	
	void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player")
            if (_log != null)
                if(!launchManager.FirstPersonController.GetComponentInChildren<ReplayRoute>().isActivated())
                    _log.InsertLiveMeasurement("EventMarker", "EnterTrigger", null, eventName);
	}

	void OnTriggerExit(Collider other) {
		if ( other.gameObject.tag == "Player" )
            if (_log != null)
                if (!launchManager.FirstPersonController.GetComponentInChildren<ReplayRoute>().isActivated())
                    _log.InsertLiveMeasurement("EventMarker", "ExitTrigger", null, eventName);
	}
}
