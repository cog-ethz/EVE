using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class TimedPopUpText : MonoBehaviour {

    private DateTime start;
    private bool started;

    [Header("User ExperimentSettings")]
    [Tooltip("Time in seconds until the displayed text is removed.")]
    public double displayReset = 1.0;

    [Tooltip("Background behind the text.")]
    public GameObject background;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (gameObject.GetComponent<Text>().text.Length != 0 & !started)
        {
            start = DateTime.Now;
            started = true;
            background.SetActive(true);
        }

        if (started)
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > displayReset)
            {
                gameObject.GetComponent<Text>().text = "";
                started = false;
                background.SetActive(false);
            }
        }
	}
}
