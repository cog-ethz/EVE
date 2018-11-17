using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ParticipantsButtons : MonoBehaviour {

        // public Texture2D closeX;
        private Vector2 scrollPosition;

        private Vector2 windowScrollPosition;

        //answer vars
        private LoggingManager log;
        private PopUpEvaluationMap map;

        private int[] session_ids;
        private string[] participant_ids;
        private string[] routes;
        private string[] files;
        private string[] temperatures, humidities;

        private int replay_session_ID;
        private int replay_sceene_ID;

        private bool[] showRoutebutton;
        private bool[] showLabChartbutton;

        private TimeSpan[][] timeSec;
        private float[][] distances;
        private string[][] envs;

        private int showInfoIndex, showInfoSSNID;

        // Use this for initialization
        void Awake()
        {
            LaunchManager launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            log = launchManager.GetLoggingManager();
            map = GameObject.Find("EvaluationMap").GetComponent<PopUpEvaluationMap>();

            //initialSkin = skin;
            string experimentName = launchManager.GetExperimentName();
            string[][] s = log.getAllSessionsData(experimentName);
            session_ids = Array.ConvertAll(s[0], a => int.Parse(a));
            participant_ids = s[1];
            files = s[3];

            showRoutebutton = new bool[session_ids.Length];
            for (int i = 0; i < session_ids.Length; i++)
                showRoutebutton[i] = (log.getXYZ(session_ids[i]) != null);

            showLabChartbutton = new bool[session_ids.Length];
            for (int i = 0; i < session_ids.Length; i++)
                showLabChartbutton[i] = File.Exists(@files[i]) ? true : false;

            timeSec = new TimeSpan[session_ids.Length][];
            distances = new float[session_ids.Length][];
            envs = new string[session_ids.Length][];

            for (int i = 0; i < session_ids.Length; i++)
            {
                if (showRoutebutton[i])
                {
                    envs[i] = log.getListOfEnvironments(session_ids[i]);
                    timeSec[i] = new TimeSpan[envs[i].Length];
                    distances[i] = new float[envs[i].Length];

                    for (int k = 0; k < envs[i].Length; k++)
                    {
                        string[] times = log.getSceneTime(k, session_ids[i]);
                        timeSec[i][k] = TimeSpan.FromSeconds(0);
                        if (times[0] != null && times[1] != null)
                            timeSec[i][k] = log.timeDifferenceTimespan(times[0], times[1]);
                        else if (times[0].Length > 0)
                        {
                            string abortTime = log.getAbortTime(session_ids[i], k);
                            if (abortTime.Length > 0)
                                timeSec[i][k] = log.timeDifferenceTimespan(times[0], abortTime);
                            else
                                timeSec[i][k] = log.timeDifferenceTimespan(times[0], times[0]); ;
                        }

                        distances[i][k] = computeDistance(session_ids[i], k);

                    }
                }
            }

            //creates a map on the second camera
            map.SetupMaps(envs);

            //delete all entries, note that this complicated procedure is needed as the enumeration of transforms changes while erasing one entry
            Transform dynamicFieldT = GameObject.Find("Evaluation Menu").GetComponent<BaseMenu>().getDynamicFields("DynFieldsA");
            List<GameObject> entriesObjects = new List<GameObject>();
            foreach (Transform entry in dynamicFieldT) entriesObjects.Add(entry.gameObject);
            foreach (GameObject entryObject in entriesObjects) Destroy(entryObject);

            for (int i = 0; i < session_ids.Length; i++)
            {
                int ssnID = session_ids[i];
                string ptcID = participant_ids[i];

                GameObject filenameObj = Instantiate(Resources.Load("Prefabs/Menus/EvaluationEntry")) as GameObject;
                Transform dynamicField = GameObject.Find("Evaluation Menu").GetComponent<BaseMenu>().getDynamicFields("DynFieldsA");
                filenameObj.transform.SetParent(dynamicField);
                filenameObj.transform.localPosition = new Vector3(filenameObj.transform.localPosition.x, filenameObj.transform.localPosition.y, dynamicField.localPosition.z);
                filenameObj.transform.localScale = new Vector3(1, 1, 1);
                Transform te = filenameObj.transform.Find("Text (1)");
                Transform te2 = filenameObj.transform.Find("Text (2)");

                UnityEngine.UI.Text tex = te.GetComponent<UnityEngine.UI.Text>();
                UnityEngine.UI.Text tex2 = te2.GetComponent<UnityEngine.UI.Text>();
                tex.text = ssnID.ToString();
                tex2.text = ptcID;
            }
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
}
