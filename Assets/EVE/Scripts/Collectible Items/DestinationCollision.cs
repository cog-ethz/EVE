using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DestinationCollision : MonoBehaviour {

    private bool reached;
    public Text popUpText;
    public string destination_Name;

	// Use this for initialization
	void Start () {
        reached = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (reached)
        {

            popUpText.text = "You have reached the " + destination_Name;
            transform.gameObject.SetActive(false);
        }  
	}

    void OnTriggerEnter(Collider other) //attached prefab
    {
        reached = true;
    }

    public bool isReached()
    {
        return reached;
    }
}
