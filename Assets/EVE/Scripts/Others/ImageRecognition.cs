using System;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
using EVE.Scripts.LevelLoader;
using UnityEngine.SceneManagement;

public class ImageRecognition : MonoBehaviour {

    private static System.Random rng = new System.Random();

    public UnityEngine.UI.Image display;
    public int maxSeconds = 10;
    public int blockNumber;

    private DateTime start;
    private int imageCounter = 0;
    private Sprite[] images;
    private string condition = "A";
    private bool loading;
    private LoggingManager log;



    // Use this for initialization
    void Start () {
        Cursor.visible = false;
        LaunchManager launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        log = launchManager.LoggingManager;
        // Get condition and order
        condition = log.GetParameterValue("Condition");
        string order = log.GetParameterValue("Order");

        string filename = "R"+ order[blockNumber - 1] + condition + "_recall";
        string[] imageNames = getImageNamesFromFile(filename);

        images = new Sprite[imageNames.Length];
        for (int i = 0; i < imageNames.Length; i++)
        {
            string name = imageNames[i].Split('.')[0];
            images[i] = Resources.Load<Sprite>("screenshots/frontal_perspectives/buildings_route_"+ order[blockNumber - 1] + "/"+name);
        }

        if (images.Length > 0)
        {
            images = randomizeList(new List<Sprite>(images)).ToArray();
            display.sprite = images[imageCounter];
        }
            
        start = DateTime.Now;

        
    }
	
	// Update is called once per frame
	void Update () {

        if (imageCounter < images.Length)
        {
            double time = DateTime.Now.Subtract(start).TotalMilliseconds;
            if ((time/1000) > maxSeconds)
            {
                String[] names = { images[imageCounter].name, images[imageCounter].name };
                String[] units = { "Answer", "miliseconds" };
                String[] values = { "2", time.ToString() };
                log.InsertLiveMeasurements("Recall", names, units, values);
                imageCounter++;
                start = DateTime.Now;
                display.sprite = images[imageCounter];               
            } else

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                String[] names = { images[imageCounter].name, images[imageCounter].name };
                String[] units = { "Filename", "miliseconds" };
                String[] values = { "0", time.ToString() };
                log.InsertLiveMeasurements("Recall", names, units, values);
                imageCounter++;
                if (imageCounter < images.Length)
                    display.sprite = images[imageCounter];
                start = DateTime.Now;               
            } else

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                String[] names = { images[imageCounter].name, images[imageCounter].name };
                String[] units = { "Filename", "miliseconds" };
                String[] values = { "1", time.ToString() };
                log.InsertLiveMeasurements("Recall", names, units, values);
                imageCounter++;
                if (imageCounter < images.Length)
                    display.sprite = images[imageCounter];
                start = DateTime.Now;               
            }            
        }	else
        {
            if (!loading)
            {
                Cursor.visible = true;
                loading = true;
                SceneManager.LoadScene("Launcher");
            }
        }


    }

    List<Sprite> randomizeList(List<Sprite> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Sprite value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public string[] getImageNamesFromFile(string filename)
    {
        TextAsset txt = (TextAsset)Resources.Load(filename, typeof(TextAsset));
        string content = txt.text;
        return Regex.Split(content, "\r\n");
    }
}
