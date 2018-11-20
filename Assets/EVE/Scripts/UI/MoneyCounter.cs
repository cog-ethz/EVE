using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MoneyCounter : MonoBehaviour {

	private Text[] money;
	
	public int startMoney;//in cent
	public bool decays;
	public float rateOfDecay;
    private LaunchManager launchManager;

    private float deltaMoney;
    private ReplayRoute rpl;

    private float fadeSpeed = 0.01f;
    private bool fadingOut;
    private float alpha;
    private float lerpTime;
    private Texture2D blackTexture;

    // Use this for initialization
    void Start () {
        launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        rpl = launchManager.FPC.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
        if (rpl.isActivated())
        {
            rateOfDecay = int.Parse(launchManager.LoggingManager.getParameterValue(launchManager.ReplaySessionId,"rateOfDecay"));
            startMoney = int.Parse(launchManager.LoggingManager.getParameterValue(launchManager.ReplaySessionId, "startMoney"));
        }
        else {            
            rateOfDecay = int.Parse(launchManager.LoggingManager.getParameterValue("rateOfDecay"));
            startMoney = int.Parse(launchManager.LoggingManager.getParameterValue("startMoney"));                
        }
        money = this.gameObject.GetComponentsInChildren<Text> ();
        deltaMoney = startMoney;
        decays = true;

        // Fading Parameters
        blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply(); 						// Apply all SetPixel calls	

        fadingOut = false;
        alpha = 0;
        lerpTime = 0;

        int expCondition = int.Parse(launchManager.LoggingManager.getParameterValue("expCondition"));
         if (expCondition == 1)
             decays = true;
         else
             decays = false;
    }
	
	// Update is called once per frame
	void Update () {
		if (decays) {
            ReplayRoute rpl = launchManager.FPC.transform.Find("PositionLogger").GetComponent<ReplayRoute>();
            if (rpl.isActivated())
            {                
                deltaMoney = startMoney - (rpl.getTimeSpent() * rateOfDecay);
               
            } else
			    deltaMoney = deltaMoney - (Time.deltaTime * rateOfDecay);
		}
        if (deltaMoney >= 0)
        {
            int displayUnit1 = (int)((deltaMoney) / 100);
            string franc = displayUnit1.ToString();
            if (displayUnit1 < 10)
            {
                franc = " " + franc;
            }
            int displayUnit2 = (int)(deltaMoney - displayUnit1 * 100);
            string cent = displayUnit2.ToString();
            if (displayUnit2 < 10)
            {
                cent = "0" + cent;
            }
            money[0].text = franc + " .";
            money[1].text = cent;
        }
		
	}

    void OnGUI()
    {
        if (deltaMoney <= 0 && !rpl.isActivated())
        {
            StartFadingIn();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);

        }

        if (fadingOut)
        {
            fadingOut = false;
            SceneManager.LoadScene("Loader");
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
