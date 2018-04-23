using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GoToDestinations : MonoBehaviour
{

    public Transform[] destinations;
    public Text popUpText;
    public double infoDelay = 1.0;

    private Transform currentDestination;
    private int destIndex = 0;
    private FadeOutScene fader;
    private bool fadingOut, changing, moneySaved;
    private DateTime start;

    private LaunchManager launchManager;
    private ReplayRoute rpl;
    private GameObject background;

    // Use this for initialization
    void Start()
    {
        currentDestination = destinations[destIndex].transform;
        gameObject.GetComponent<Text>().text = "Find the " + currentDestination.name;
        currentDestination.transform.Find("Cylinder").gameObject.SetActive(true);
        fader = GameObject.FindGameObjectWithTag("Player").GetComponent<FadeOutScene>();
        start = DateTime.Now;
        currentDestination = destinations[destIndex].transform;
        popUpText.text = "Find the " + currentDestination.name;
        background = popUpText.transform.parent.GetChild(0).gameObject;
    }

    void OnGUI()
    {
        if (fadingOut)
            fader.startFadeOut();

    }

    // Update is called once per frame
    void Update()
    {
        if (changing)
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > infoDelay)
            {
                currentDestination = destinations[destIndex].transform;
                popUpText.text = "Find the " + currentDestination.name;
                gameObject.GetComponent<Text>().text = "Find the " + currentDestination.name;
                //currentDestination.gameObject.SetActive(true);
                currentDestination.transform.Find("Cylinder").gameObject.SetActive(true);
                changing = false;
                background.SetActive(true);
            }
        }
        else
        {
            if (fader.isFadedOut())
            {

                launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
                rpl = launchManager.FPC.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
                if (rpl.isActivated())
                    SceneManager.LoadScene("Evaluation");
                else
                    SceneManager.LoadScene("Loader");
            }
            if (!currentDestination.transform.Find("Cylinder").gameObject.activeSelf)
            {
                destIndex++;
                if (destIndex < destinations.Length)
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
}
