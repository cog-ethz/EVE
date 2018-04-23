using UnityEngine;
using System.Collections;

public class FadeOutScene : MonoBehaviour
{

    private float fadeSpeed = 0.01f;
    private bool fadingOut;
    private float alpha;
    private float lerpTime;
    private Texture2D blackTexture;

    // Use this for initialization
    void Start()
    {
        // Fading Parameters
        blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply(); 						// Apply all SetPixel calls	

        //fadingIn = true;
        fadingOut = false;
        alpha = 0;
        lerpTime = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startFadeOut()
    {
        StartFading();
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
    }

    public bool isFadedOut()
    {
        return fadingOut;
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

    void StartFading()
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
