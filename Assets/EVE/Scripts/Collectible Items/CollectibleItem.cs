using UnityEngine;
using System.Collections;

public class CollectibleItem : MonoBehaviour {

	private static int collectedItems = 0;

    public string id;

    private LoggingManager log;

    void Start()
    {
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        log = launchManager.GetLoggingManager();
        collectedItems = 0;
    }

	void Awake () {
		transform.GetComponent<Collider>().isTrigger = true;
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "Player")
		{
			transform.gameObject.GetComponent<Collider>().enabled = false;
			transform.gameObject.GetComponent<Renderer>().enabled = false;
            if (log != null) log.insertLiveMeasurement("Collectible","Gem",null,id);
			Destroy(this.gameObject);
			collectedItems++;
            
		}
	}

	public static int getCollectedItems() {
		return collectedItems;
	}

	public static void resetCollectedItems() {
		collectedItems = 0;
	}
}
