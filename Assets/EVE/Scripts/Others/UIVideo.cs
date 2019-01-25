using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIVideo : MonoBehaviour {
    
    public VideoClip[] movies;
    public RawImage textureScreen;

    private VideoClip mov;
    private UnityEngine.Video.VideoPlayer videoPlayer;
    private int movIdx;

    // Use this for initialization
    void Start ()
    {
        movIdx = 0;
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.clip = movies[movIdx];
        videoPlayer.isLooping = true;
        StartCoroutine(setupVideo());

    }

    IEnumerator setupVideo()
    {
        videoPlayer.Prepare();
        textureScreen.color = new Color(1f, 1f, 1f, 0f);
        WaitForSeconds waitTime = new WaitForSeconds(1);
        while (!videoPlayer.isPrepared)
        {
            yield return waitTime;
            break;
        }
        textureScreen.texture = videoPlayer.texture;
        textureScreen.color = new Color(1f, 1f, 1f, 1f);
        videoPlayer.Play();
        yield return null;
    }

    public void hideVideo()
    {
        textureScreen.gameObject.SetActive(false);
    }
	
    public void playVideo()
    {
        videoPlayer.Play();
    }

    public void pauseVideo()
    {
        videoPlayer.Pause();
    }

    public void switchToNextMovie()
    {       
        videoPlayer.Pause();
        movIdx++;
        if (movIdx < movies.Length)
        {
            videoPlayer.clip = movies[movIdx];
        }

        StartCoroutine(setupVideo());        
    }


}
