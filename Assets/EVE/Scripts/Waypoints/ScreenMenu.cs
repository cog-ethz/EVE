using UnityEngine;
using System.Collections;

public class ScreenMenu : MonoBehaviour {

	public Texture 		backgroundTexture;
	public TextAsset 	content;
	public GUIStyle 	contentSkin;
	public float 		fadeSpeed;

	public static bool 	showFloatingMenus;
	
	public float 		menuXpercent;
	public float 		menuYpercent;
	public  float 		menuWidth;
	public  float 		menuHeight;

	private bool  		fadingIn;
	private bool  		fadingOut;
	private float 		alpha;
	private float 		lerpTime;

	public Camera 		cam;
	public GameObject 	player;

	void Awake () {
		// Fading Parameters
		fadingIn = true;
		fadingOut = false;
		alpha = 0;
		lerpTime = 0;

		//show floating menu
		showFloatingMenus = true;
	}

	void Update () {		
	}

	void OnGUI () {
		//draw stuff
		if( showFloatingMenus ) {
			if (IsInsideMenuArea ()) {
				if (fadingIn)  StartFadingIn ();	// controls the transparency of the drawn elements
				DisplayMenu ();
			} else {
				if(fadingOut)  {
					StartFadingOut();
					DisplayMenu ();
				}
			} 
		}
	}

	// -----------------------------------------
	//			 public functions 
	//------------------------------------------

	public float DistanceToPlayer() {
		return (transform.position - player.transform.position).magnitude;
	}

	public bool IsInsideMenuArea() {
		return ( DistanceToPlayer() < GetComponent<SphereCollider>().radius );
		//return ( GetComponent<BoxCollider>(). );
	}

	// -----------------------------------------
	//			 display functions 
	//------------------------------------------

	void DisplayMenu() {
		GUI.depth = 10;
		GUI.contentColor = Color.black;

		// calculate position
		float menuX = Screen.width * menuXpercent  - menuWidth/2;
		float menuY = Screen.height * menuYpercent - menuHeight/2;

		//draw background texture
		GUI.DrawTexture (new Rect (menuX, menuY, menuWidth, menuHeight), backgroundTexture);
		//draw text
		GUI.Box (new Rect (menuX, menuY, menuWidth, menuHeight), content.ToString(), contentSkin);

	}

	// -----------------------------------------
	//			fading in and out functions
	//------------------------------------------

	void FadeToMenu()
	{
		lerpTime += fadeSpeed * Time.deltaTime;
		alpha = Mathf.Lerp (alpha, 1f, lerpTime);
		GUI.color = new Color(1,1,1,alpha);
	}

	void FadeToClear()
	{
		lerpTime += fadeSpeed * Time.deltaTime;
		alpha = Mathf.Lerp (alpha, 0, lerpTime);
		GUI.color = new Color(1,1,1,alpha);
	}

	void StartFadingIn()
	{
		FadeToMenu();
		
		if (GUI.color.a >= 0.95f) {
			GUI.color = new Color(1,1,1,1);
			fadingIn = false;
			fadingOut = true;
			lerpTime = 0;
		}
	}

	void StartFadingOut()
	{
		FadeToClear();
		
		if (GUI.color.a <= 0.05f) {
			GUI.color = new Color(1,1,1,0);
			fadingIn = true;
			fadingOut = false;
			lerpTime = 0;
		}
	}
}
