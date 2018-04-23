using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayButton : MonoBehaviour {
    private int replaySceneID;
    private string replaySceneName;
    private int replaySessionID;

    public void onClickReplay() {
        GameObject FPC = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>().FPC;
        ReplayRoute replay = FPC.GetComponentInChildren<ReplayRoute>();
        replay.activateReplay(replaySessionID, replaySceneName, replaySceneID);
        SceneManager.LoadScene(replaySceneName);
    }

    public void setreplaySceneID(int i)
    {
        replaySceneID = i;
    }

    public void setReplaySessionId(int value)
    {
        replaySessionID = value;
    }

    public void setReplaySceneName(string value)
    {
        replaySceneName = value;
    }
}
