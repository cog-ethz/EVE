using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour {

	private Text[] time;

	private float deltaTime, maxDuration;

    private float fadeSpeed = 0.01f;
    private bool fadingOut;
    private float alpha;
    private float lerpTime;
    private Texture2D blackTexture;

    private LaunchManager launchManager;
    private ReplayRoute rpl;
    private bool _once;

    // Use this for initialization
    void Start () {
        // Fading Parameters
        blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply(); 						// Apply all SetPixel calls	

        fadingOut = false;
        alpha = 0;
        lerpTime = 0;

        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        rpl = launchManager.FirstPersonController.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
        if (rpl.isActivated())
        {
            maxDuration = int.Parse(launchManager.LoggingManager.GetParameterValue(launchManager.ReplaySessionId,"maxDuration"));
        }
        else
        {
            maxDuration = int.Parse(launchManager.LoggingManager.GetParameterValue("maxDuration"));
        }

        time = this.gameObject.GetComponentsInChildren<Text> ();        
        deltaTime = maxDuration;
    }
	
	// Update is called once per frame
	void Update () {
        rpl = launchManager.FirstPersonController.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
        if (rpl.isActivated()) { 
            deltaTime = maxDuration - rpl.getTimeSpent();
        }
        else
		    deltaTime = deltaTime-Time.deltaTime;
        if (deltaTime >= 0)
        {
            int displayMin = ((int)deltaTime) / 60;
            string min = displayMin.ToString();
            if (displayMin < 10)
            {
                min = "0" + min;
            }
            int displaySec = (int)(deltaTime - displayMin * 60);
            string sec = displaySec.ToString();
            if (displaySec < 10)
            {
                sec = "0" + sec;
            }
            time[0].text = min + " :";
            time[1].text = sec;           
        }
	}

    void OnGUI()
    {
        if (deltaTime <= 0 && !rpl.isActivated())
        {
            StartFadingIn();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);

        }

        if (fadingOut)
        {
            fadingOut = false;

            if (_once) return;
            _once = true;
            SceneManager.LoadScene("Launcher");           
        }
    }

    // -----------------------------------------
    //			fading in and out functions
    //------------------------------------------

    void FadeToMenu()
    {
        lerpTime += fadeSpeed * Time.deltaTime;
        alpha = Mathf.Lerp(alpha, 1f, lerpTime);
        GUI.color = new Color(1, 1, 1, alpha);
    }

    void StartFadingIn()
    {
        FadeToMenu();

        if (GUI.color.a >= 0.95f)
        {
            GUI.color = new Color(1, 1, 1, 1);
            fadingOut = true;
            lerpTime = 0;
        }
    }
}

