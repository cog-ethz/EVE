using UnityEngine;
using System.Collections;

public class ReachDestination : MonoBehaviour {

	private CrossOfElement destinationList;

	private int index;
	public string destinationName;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "Player")
		{
			//transform.gameObject.GetComponent<Collider>().enabled = false;
			//transform.gameObject.GetComponent<Renderer>().enabled = false;
			destinationList.StrikeOff(index);
			//Destroy(this.gameObject);
		}
	}

	public void setIndex(int i){
		this.index = i;
	}

	public void setDestinationList(CrossOfElement destinationList){
		this.destinationList = destinationList;
	}
}
