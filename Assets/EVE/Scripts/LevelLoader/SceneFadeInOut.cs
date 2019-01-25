using UnityEngine;
using System.Collections;

public class SceneFadeInOut : MonoBehaviour {
	public float fadeInSpeed = 1.5f;
	public float fadeOutSpeed = 0.4f;

	public bool sceneStarting = false;

	void Awake() 
	{
		GetComponent<GUITexture>().pixelInset = new Rect (0f, 0f, 9000, 9000); //Screen.width, Screen.height);
	}

	void Update() 
	{
		if (sceneStarting) 
		{
			StartScene();
		}
	}

	void FadeToClear()
	{
		GetComponent<GUITexture>().color = Color.Lerp (GetComponent<GUITexture>().color, Color.clear, fadeInSpeed * Time.deltaTime);
	}

	void FadeToBlack()
	{
		GetComponent<GUITexture>().color = Color.Lerp (GetComponent<GUITexture>().color, Color.black, fadeOutSpeed * Time.deltaTime);
	}

	void StartScene()
	{
		FadeToClear ();

		if (GetComponent<GUITexture>().color.a <= 0.05f) {
  			GetComponent<GUITexture>().color = Color.clear;
			GetComponent<GUITexture>().enabled = false;
			sceneStarting = false;				
		}
	}

	public void EndScene()
	{
		sceneStarting = false;
		GetComponent<GUITexture>().enabled = true;
		FadeToBlack ();

		if (GetComponent<GUITexture>().color.a >= 0.95f) {
			GetComponent<GUITexture>().color = new Color(1,1,1,1);
		}
	}




}
