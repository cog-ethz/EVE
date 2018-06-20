using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionScreen : MonoBehaviour {

    public int waitTime = 0;

    private DateTime start;
    private GameObject nextButton, controlSectionText;

    private bool pressedOnce = false, loading = false;

    // Use this for initialization
    void Start()
    {
        start = DateTime.Now;
        GameObject controlSection = this.gameObject.transform.Find("Panel").transform.Find("controlSection").gameObject;
        nextButton = controlSection.transform.Find("controlButtons").transform.Find("NextButton").gameObject;
        controlSectionText = controlSection.transform.Find("ContinueInstructions").gameObject;
        controlSectionText.GetComponent<UnityEngine.UI.Text>().text = "Press when you are ready to continue";
        nextButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (DateTime.Now.Subtract(start).TotalSeconds > waitTime)
        {
            nextButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
            if (Input.GetButtonUp("Back"))
            {
                pressContinue();
            }
        }
       
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.End)
        {
            SceneManager.LoadScene("Loader");
        }
    }


    public void pressContinue()
    {
        if (!pressedOnce)
        {
            pressedOnce = true;
            controlSectionText.GetComponent<UnityEngine.UI.Text>().text = "Press again to confirm";
        } else
        {
            if (!loading)
            {
                loading = true;
                SceneManager.LoadScene("Loader");
            }
        }
    }
}
