using UnityEngine;
using System.Collections;

public class EventMarker: MonoBehaviour {

	public string 		eventName;
	
	public Camera 		cam;
	public GameObject 	player;
	private LoggingManager log;

	void Start() {
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
         log = launchManager.GetLoggingManager();
	}
	
	void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player")
            if (log != null) log.insertLiveMeasurement("EventMarker", "EnterTrigger", null, eventName);
	}

	void OnTriggerExit(Collider other) {
		if ( other.gameObject.tag == "Player" )
            if (log != null) log.insertLiveMeasurement("EventMarker", "ExitTrigger", null, eventName);
	}
}
