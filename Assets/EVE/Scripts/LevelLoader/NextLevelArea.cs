using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NextLevelArea : MonoBehaviour {

	private float 		fadeSpeed = 0.01f;
	private bool  		fadingIn;
	private bool  		fadingOut;
	private bool 		exitArea = false;
	private float 		alpha;
	private float 		lerpTime;
	private Texture2D   blackTexture;
	private GameObject 	player;
	private Vector3 	menuWorldPosition;
    private bool _once;

    void Awake () {
		// Fading Parameters
		blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		blackTexture.SetPixel(0, 0, Color.black);
		blackTexture.Apply(); 						// Apply all SetPixel calls	

		fadingIn = true;
		fadingOut = false;
		alpha = 0;
		lerpTime = 0;
        
        player = GameObject.FindGameObjectWithTag("Player");
    }

	void Update () {		
	}

	void OnGUI() {
		//draw stuff
		if (IsInsideMenuArea ()) {
			exitArea = true;
		} 

		if (exitArea) {
			if (fadingIn)  StartFadingIn ();
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), blackTexture);
		}

		if (fadingOut) {	//fading in to black is finished
			CollectibleItem.resetCollectedItems();
            fadingOut = false;
            if (_once) return;
            _once = true;
            SceneManager.LoadScene("Loader");
		}
	}

	public float DistanceToPlayer() {   
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
		return (transform.position - player.transform.position).magnitude;
	}

	public bool IsInsideMenuArea() {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        return ( DistanceToPlayer() < GetComponent<SphereCollider>().radius );
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

	void StartFadingIn()
	{
		FadeToMenu();

        if (GUI.color.a >= 0.95f && GUI.color.a != 0.95f)
        {
			GUI.color = new Color(1,1,1,1);
			fadingIn = false;
			fadingOut = true;
			lerpTime = 0;
		}
	}
}
