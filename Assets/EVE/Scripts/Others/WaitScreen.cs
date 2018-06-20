using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class WaitScreen : MonoBehaviour {

 public int waitTime = 0;

    private DateTime start;
    private bool loading = false; 

	// Use this for initialization
	void Start () {
        start = DateTime.Now;
    }


    void OnGUI()
    {

    if (DateTime.Now.Subtract(start).TotalSeconds > waitTime)
        if (!loading)
        {
            loading = true;
            SceneManager.LoadScene("Loader");
        }


        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Loader");
        }
    }
	
}
