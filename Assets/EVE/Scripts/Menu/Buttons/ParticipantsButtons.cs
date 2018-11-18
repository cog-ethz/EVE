using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ParticipantsButtons : MonoBehaviour {

        // public Texture2D closeX;
        private Vector2 scrollPosition;

        private Vector2 windowScrollPosition;

        //answer vars
        private LoggingManager log;

        private GameObject _map;
        //private PopUpEvaluationMap map;

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
        void Start()
        {
            var launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            log = launchManager.GetLoggingManager();

            _map = Instantiate(Resources.Load("Prefabs/Menus/Evaluation/EvaluationMap")) as GameObject;

            //initialSkin = skin;
            var experimentName = launchManager.GetExperimentName();
            var s = log.getAllSessionsData(experimentName);
            session_ids = Array.ConvertAll(s[0], int.Parse);
            participant_ids = s[1];
            files = s[3];

            showRoutebutton = new bool[session_ids.Length];
            showLabChartbutton = new bool[session_ids.Length];

            timeSec = new TimeSpan[session_ids.Length][];
            distances = new float[session_ids.Length][];
            envs = new string[session_ids.Length][];


            var dynamicField = GameObject.Find("Participants Menu").GetComponent<BaseMenu>().getDynamicFields("DynFields");

            for (var i = 0; i < session_ids.Length; i++)
            {
                var sid = session_ids[i];

                showRoutebutton[i] = log.getXYZ(sid).Any();
                showLabChartbutton[i] = File.Exists(@files[i]);

                if (!showRoutebutton[i]) continue;
                envs[i] = log.getListOfEnvironments(sid);
                timeSec[i] = new TimeSpan[envs[i].Length];
                distances[i] = new float[envs[i].Length];

                for (var k = 0; k < envs[i].Length; k++)
                {
                    var times = log.getSceneTime(k, sid);
                    timeSec[i][k] = TimeSpan.FromSeconds(0);
                    if (times[0] != null && times[1] != null)
                        timeSec[i][k] = log.timeDifferenceTimespan(times[0], times[1]);
                    else if (times[0].Length > 0)
                    {
                        //string abortTime = log.getAbortTime(sid, k);
                        //if (abortTime.Length > 0)
                        //    timeSec[i][k] = log.timeDifferenceTimespan(times[0], abortTime);
                        //else
                        timeSec[i][k] = log.timeDifferenceTimespan(times[0], times[0]); ;
                    }
                    distances[i][k] = computeDistance(sid, k);
                }

                var pid = participant_ids[i];

                var gObject = Instantiate(Resources.Load("Prefabs/Menus/EvaluationEntry")) as GameObject;
                Utils.PlaceElement(gObject, dynamicField);

                gObject.transform.Find("SessionId").GetComponent<Text>().text = sid.ToString();
                gObject.transform.Find("ParticipantId").GetComponent<Text>().text = pid;
            }

            //creates a map on the second camera
            _map.GetComponent<PopUpEvaluationMap>().SetupMaps(envs);
        }

        private float computeDistance(int sessionID, int sceneID)
        {
            float distance = 0; ;
            var xyz_table = log.getXYZ(sessionID, sceneID);
            for (var i = 1; i < xyz_table[0].Count; i++)
            {
                var old = new Vector3(xyz_table[0][i - 1], xyz_table[1][i - 1], xyz_table[2][i - 1]);
                var current = new Vector3(xyz_table[0][i], xyz_table[1][i], xyz_table[2][i]);
                distance += (current - old).magnitude;
            }
            return distance;
        }
    }
}
