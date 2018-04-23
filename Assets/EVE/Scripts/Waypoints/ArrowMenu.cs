using UnityEngine;
using System.Collections;

public class ArrowMenu : MonoBehaviour {

	public Texture 		backgroundTexture;
	public TextAsset 	content;
	public GUIStyle 	contentSkin;
	public float 		fadeSpeed;

	public static bool 	showFloatingMenus;
	
	public Material 	inactiveMaterial;
	public Material 	activeMaterial;

	private bool  		fadingIn;
	private bool  		fadingOut;
	private float 		alpha;
	private float 		r;
	private float 		g;
	private float 		b;
	private float 		lerpTime;
	public Color		color;

	public Camera 		cam;
	public GameObject 	player;
	public Transform    flashingObject;

	void Awake () {
		flashingObject = transform.Find("game_object"); 

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
	}

	// -----------------------------------------
	//			 display functions 
	//------------------------------------------

	void DisplayMenu() {

	}

	// -----------------------------------------
	//			fading in and out functions
	//------------------------------------------

	void FadeToMenu()
	{
		lerpTime += fadeSpeed * Time.deltaTime;
		alpha 	  = Mathf.Lerp (alpha, 1f, lerpTime);
		color 	  = flashingObject.transform.GetComponent<Renderer>().material.color;
		r 		  = Mathf.Lerp (color.r, activeMaterial.color.r, lerpTime);
		g 		  = Mathf.Lerp (color.g, activeMaterial.color.g, lerpTime);
		b 		  = Mathf.Lerp (color.b, activeMaterial.color.b, lerpTime);
		flashingObject.transform.GetComponent<Renderer>().material.color = new Color(r,g,b,alpha);
	}

	void FadeToClear()
	{
		lerpTime += fadeSpeed * Time.deltaTime;
		alpha 	  = Mathf.Lerp (alpha, 0, lerpTime);
		color 	  = flashingObject.transform.GetComponent<Renderer>().material.color;
		r 		  = Mathf.Lerp (color.r, inactiveMaterial.color.r, lerpTime);
		g 		  = Mathf.Lerp (color.g, inactiveMaterial.color.g, lerpTime);
		b 		  = Mathf.Lerp (color.b, inactiveMaterial.color.b, lerpTime);
		flashingObject.transform.GetComponent<Renderer>().material.color = new Color(r,g,b,alpha);
	}

	void StartFadingIn()
	{
		FadeToMenu();
		
		if (alpha >= 0.95f) {
			alpha = 1f;
			color = activeMaterial.color;
			flashingObject.transform.GetComponent<Renderer>().material.color = activeMaterial.color;
			fadingIn = false;
			fadingOut = true;
			lerpTime = 0;
		}
	}

	void StartFadingOut()
	{
		FadeToClear();
		
		if (alpha <= 0.05f) {
			alpha = 0f;
			color = inactiveMaterial.color;
			flashingObject.transform.GetComponent<Renderer>().material.color = inactiveMaterial.color;
			fadingIn = true;
			fadingOut = false;
			lerpTime = 0;
		}
	}
}
