using System;
using System.Collections;
using System.Collections.Generic;
using Assets.EVE.Scripts.Menu;
using UnityEngine;


public class UpdateEvalDetailList : MonoBehaviour {

    int internalSessionID=-1;
    private MenuManager menumanager;
    private LoggingManager log;
    private PopUpEvaluationMap map;

    void Start() {
        GameObject objectContainingMenuManager = GameObject.Find("Canvas");
        menumanager = objectContainingMenuManager.GetComponent<MenuManager>();
        log = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>().GetLoggingManager();
    }

    void Update() {

        if (menumanager.getDetailsInt() != internalSessionID)
        {
            internalSessionID = menumanager.getDetailsInt();
            //wipe current interior
            Transform dynamicFieldT = GameObject.Find("Participant Menu").GetComponent<BaseMenu>().getDynamicFields("DynFields");
            List<GameObject> entriesObjects = new List<GameObject>();
            foreach (Transform entry in dynamicFieldT) entriesObjects.Add(entry.gameObject);
            foreach (GameObject entryObject in entriesObjects) Destroy(entryObject);

            int sessionID = menumanager.getDetailsInt();

            string[] envs = log.getListOfEnvironments(sessionID);
            TimeSpan[] timeSec = new TimeSpan[envs.Length];

            instantiateTextWithText("Session ID", sessionID.ToString());

            for (int k=0;k<envs.Length ;k++) {
                instantiateTextWithText("Scene", "Scene "+k+": " + envs[k]);
                timeSec[k] = TimeSpan.FromSeconds(0);
                string[] times = log.getSceneTime(k, sessionID);
                if (times[0] != null && times[1] != null)
                    timeSec[k] = log.timeDifferenceTimespan(times[0], times[1]);
                else if (times[0].Length > 0){
                    //string abortTime = log.getAbortTime(sessionID, k);
                    //if (abortTime.Length > 0)
                    //    timeSec[k] = log.timeDifferenceTimespan(times[0], abortTime);
                    //else
                    timeSec[k] = log.timeDifferenceTimespan(times[0], times[0]);
                }
                instantiateTextWithTextAndTab("Time", timeSec[k].TotalSeconds.ToString());

                if (log.getXYZ(sessionID, k)[0].Count > 0)
                {

                    float distance = computeDistance(sessionID, k);
                    instantiateTextWithTextAndTab("Distance", distance.ToString());

                    //make replay button
                    GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TabTabAndReplayButton")) as GameObject;
                    Transform dynamicField = GameObject.Find("Evaluation Details").GetComponent<BaseMenu>().getDynamicFields("DynFieldsA");
                    filenameObj.transform.SetParent(dynamicField);
                    filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
                    filenameObj.transform.localScale = new Vector3(1, 1, 1);
                    filenameObj.transform.Find("Button").GetComponent<ReplayButton>().setreplaySceneID(k);
                    filenameObj.transform.Find("Button").GetComponent<ReplayButton>().setReplaySceneName(envs[k]);
                    filenameObj.transform.Find("Button").GetComponent<ReplayButton>().setReplaySessionId(sessionID);

                    //make show map button
                    map = gameObject.GetComponent<PopUpEvaluationMap>();
                    //map.SetupMaps(envNumber);
                    GameObject filenameObj2 = Instantiate(Resources.Load("Prefabs/Menus/TabTabAndShowMapButton")) as GameObject;
                    filenameObj2.transform.SetParent(dynamicField);
                    filenameObj2.transform.localPosition = new Vector3(filenameObj2.transform.localPosition.x, filenameObj2.transform.localPosition.y, dynamicField.localPosition.z);
                    filenameObj2.transform.localScale = new Vector3(1, 1, 1);
                    var showMapButtonScript = filenameObj2.transform.Find("Button").GetComponent<ShowMapButton>();
                    showMapButtonScript.setupButton();
                    showMapButtonScript.setEnvNumber(k);
                    showMapButtonScript.setEnvName(envs[k]);
                    showMapButtonScript.setSessionID(sessionID);
                    showMapButtonScript.setMap(map);
                }               
            }



        }
    }

    public void instantiateTextWithText(string evalField,string evalValue){
        GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TextWithText")) as GameObject;
        Transform dynamicField = GameObject.Find("Participant Menu").GetComponent<BaseMenu>().getDynamicFields("DynFields");
        filenameObj.transform.SetParent(dynamicField);
        filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
        filenameObj.transform.localScale = new Vector3(1, 1, 1);
        Transform te = filenameObj.transform.Find("evalField");
        UnityEngine.UI.Text tex = te.GetComponent<UnityEngine.UI.Text>();
        tex.text = evalField;
        Transform te2 = filenameObj.transform.Find("evalValue");
        UnityEngine.UI.Text tex2 = te2.GetComponent<UnityEngine.UI.Text>();
        tex2.text = evalValue;
    }

    public void instantiateTextWithTextAndTab(string evalField, string evalValue)
    {
        GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TextWithTextAndTab")) as GameObject;
        Transform dynamicField = GameObject.Find("Participant Menu").GetComponent<BaseMenu>().getDynamicFields("DynFields");
        filenameObj.transform.SetParent(dynamicField);
        filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
        filenameObj.transform.localScale = new Vector3(1, 1, 1);
        Transform te = filenameObj.transform.Find("evalField");
        UnityEngine.UI.Text tex = te.GetComponent<UnityEngine.UI.Text>();
        tex.text = evalField;
        Transform te2 = filenameObj.transform.Find("evalValue");
        UnityEngine.UI.Text tex2 = te2.GetComponent<UnityEngine.UI.Text>();
        tex2.text = evalValue;
    }


    private float computeDistance(int sessionID, int sceneID)
    {
        float distance = 0; ;
        List<float>[] xyz_table = log.getXYZ(sessionID, sceneID);

        for (int i = 1; i < xyz_table[0].Count; i++)
        {
            Vector3 old = new Vector3(xyz_table[0][i - 1], xyz_table[1][i - 1], xyz_table[2][i - 1]);
            Vector3 current = new Vector3(xyz_table[0][i], xyz_table[1][i], xyz_table[2][i]);
            distance += (current - old).magnitude;
        }

        return distance;
    }
}
