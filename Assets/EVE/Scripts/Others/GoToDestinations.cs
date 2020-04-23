using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using EVE.Scripts.LevelLoader;

public class GoToDestinations : MonoBehaviour
{
    public GameObject DestinationParent;
    //public Transform[] destinations;
    private Text popUpText;
    public double infoDelay = 1.0;

    private Transform currentDestination;
    private int destIndex = 0;
    private FadeOutScene fader;
    private bool fadingOut, changing, moneySaved;
    private DateTime start;

    private LaunchManager launchManager;
    private ReplayRoute rpl;
    private GameObject background;

    void Start()
    {
        fader = GameObject.FindGameObjectWithTag("Player").GetComponent<FadeOutScene>();
        currentDestination = DestinationParent.transform.GetChild(destIndex);
        popUpText = gameObject.GetComponent<Text>();
        currentDestination.transform.Find("Cylinder").gameObject.SetActive(true);
        start = DateTime.Now;
        currentDestination = DestinationParent.transform.GetChild(destIndex);
        popUpText.text = "Find the " + currentDestination.name;
        background = popUpText.transform.parent.GetChild(0).gameObject;
        changing = true;
    }

    void OnGUI()
    {
        if (fadingOut)
            fader.startFadeOut();

    }

    void Update()
    {
        if (fader == null)
            fader = GameObject.FindGameObjectWithTag("Player").GetComponent<FadeOutScene>();
        if (background)
            background = popUpText.transform.parent.GetChild(0).gameObject;
        if (changing)
        {
            if (!(DateTime.Now.Subtract(start).TotalSeconds > infoDelay)) return;
            currentDestination = DestinationParent.transform.GetChild(destIndex);
            popUpText.text = "Find the " + currentDestination.name;
            gameObject.GetComponent<Text>().text = "Find the " + currentDestination.name;
            currentDestination.transform.Find("Cylinder").gameObject.SetActive(true);
            changing = false;
            background.SetActive(true);
        }
        else
        {
            if (fader.isFadedOut())
            {               
                SceneManager.LoadScene("Launcher");
            }

            if (currentDestination.transform.Find("Cylinder").gameObject.activeSelf) return;
            
            destIndex++;
            if (destIndex < DestinationParent.transform.childCount)
            {
                changing = true;
                start = DateTime.Now;
            }
            else
            {
                fadingOut = true;                  
            }
        }
    }
}
