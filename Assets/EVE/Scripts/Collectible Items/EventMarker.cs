using UnityEngine;
using System.Collections;

public class EventMarker: MonoBehaviour {

	public string eventName;
	private LoggingManager _log;
    private LaunchManager launchManager;



    void Start() {
        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
	    _log = launchManager.GetLoggingManager();
	}
	
	void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player")
            if (_log != null)
                if(!launchManager.FPC.GetComponentInChildren<ReplayRoute>().isActivated())
                    _log.insertLiveMeasurement("EventMarker", "EnterTrigger", null, eventName);
	}

	void OnTriggerExit(Collider other) {
		if ( other.gameObject.tag == "Player" )
            if (_log != null)
                if (!launchManager.FPC.GetComponentInChildren<ReplayRoute>().isActivated())
                    _log.insertLiveMeasurement("EventMarker", "ExitTrigger", null, eventName);
	}
}
