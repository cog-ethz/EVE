using UnityEngine;
using System.Collections;

public class ButtonSetup : MonoBehaviour {

	public Texture 	tex3;
	public Texture 	tex4;
	public Texture 	tex5;
	public Texture 	tex6;
	public Texture 	tex;

	void Awake () {
	}

	void Update () {		
	}

	void OnGUI () {
		//draw stuff
		if( Input.GetButton("Top3")) {			
			GUI.DrawTexture (new Rect (300, 300, 200, 200), tex3);
		}

		if( Input.GetButton("Top4")) {			
			GUI.DrawTexture (new Rect (300, 300, 200, 200), tex4);
		}

		if( Input.GetButton("Top5") ) {			
			GUI.DrawTexture (new Rect (300, 300, 200, 200), tex5);
		}

		if( Input.GetButton("Top6") ) {			
			GUI.DrawTexture (new Rect (300, 300, 200, 200), tex6);
		}

		if( Input.GetButton("Back")) {			
			GUI.DrawTexture (new Rect (300, 300, 200, 200), tex);
		}
	}
}
