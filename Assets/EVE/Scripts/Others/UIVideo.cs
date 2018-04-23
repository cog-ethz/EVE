using UnityEngine;
using System.Collections;

public class UIVideo : MonoBehaviour {
    public int vidWidth, vidHeight;
    public MovieTexture[] movies;
    public bool setPosition = false, rightAlligned = false , baselineVideo = false, loop=true ;
    public int pX, pY;
    private Texture2D blackTexture;
    

    private float posX, posY;
    private MovieTexture mov;
    private int movIdx;
    private bool showVid;

	// Use this for initialization
	void Start () {
        movIdx = 0;
        mov = movies[movIdx];
        mov.loop = loop;
        if (setPosition)
        {
            if (rightAlligned)
            {
                posX = Screen.width - pX;
                posY = pY;
            }
            else {
                posX = pX;
                posY = pY;
            } 
        }
        else
        {
            posX = (Screen.width - vidWidth)/2;
            posY = (Screen.height - vidHeight)/2;
        }
        playVideo();
        showVideo();
        blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply(); 	
	}
	
	// Update is called once per frame
	void Update () {
      /*  if (Input.GetButtonDown("Jump"))
        {
            if (mov.isPlaying)
            {
                pauseVideo();
            }
            else
            {
                playVideo();
            }
        }*/
	}

    void OnGUI()
    {
        if (showVid)
        {
            if (baselineVideo)
            {
                // For 720
                /*GUI.DrawTexture(new Rect(posX, posY-10, vidWidth, vidHeight), mov);
                GUI.DrawTexture(new Rect(posX, posY-10, vidWidth, 20), blackTexture);*/
                

                // For 1080
                if (mov.isPlaying)
                {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
                    GUI.DrawTexture(new Rect(posX, posY - 13, vidWidth, vidHeight), mov);
                    GUI.DrawTexture(new Rect(posX, posY - 13, vidWidth, 26), blackTexture);
                }
                
            }
            else {
                GUI.DrawTexture(new Rect(posX, posY, vidWidth, vidHeight), mov);
            }
        }
            
    }

    public void playVideo()
    {
        if (mov!= null)
            mov.Play();
    }

    public void pauseVideo()
    {
        if (mov!= null)
            mov.Pause();
    }

    public void switchToNextMovie()
    {
        mov.Stop();
        movIdx++;
        if (movIdx < movies.Length)
        {
            mov = movies[movIdx];
            mov.loop = true;
        }
    }

    public void showVideo()
    {
        showVid = true;
    }

    public void hideVideo()
    {
        showVid = false;
    }
}
