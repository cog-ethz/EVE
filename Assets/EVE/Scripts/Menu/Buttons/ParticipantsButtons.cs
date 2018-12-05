using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ParticipantsButtons : MonoBehaviour {

        // public Texture2D closeX;
        private Vector2 scrollPosition;

        private Vector2 windowScrollPosition;

        //answer vars
        private LoggingManager _log;

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
        private LaunchManager _launchManager;
        private MenuManager _menuManager;
        private Transform _dynFields;


        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _log = _launchManager.LoggingManager;

            _map = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Evaluation/EvaluationMap");



            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Evaluation Menu", "Launcher"); });

            _dynFields = fields.Find("DynFieldsWithScrollbar").Find("DynFields");

            DisplayParticipants();
        }

        public void DisplayParticipants()
        {
            MenuUtils.ClearList(_dynFields);

            var experimentName = _launchManager.ExperimentName;
            var s = _log.getAllSessionsData(experimentName);
            session_ids = Array.ConvertAll(s[0], int.Parse);
            participant_ids = s[1];
            files = s[3];

            showRoutebutton = new bool[session_ids.Length];
            showLabChartbutton = new bool[session_ids.Length];

            timeSec = new TimeSpan[session_ids.Length][];
            distances = new float[session_ids.Length][];
            envs = new string[session_ids.Length][];
            
            for (var i = 0; i < session_ids.Length; i++)
            {
                var sid = session_ids[i];

                showRoutebutton[i] = _log.GetPath(sid).Any();
                showLabChartbutton[i] = File.Exists(@files[i]);

                if (!showRoutebutton[i]) continue;
                envs[i] = _log.getListOfEnvironments(sid);
                timeSec[i] = new TimeSpan[envs[i].Length];
                distances[i] = new float[envs[i].Length];

                for (var k = 0; k < envs[i].Length; k++)
                {
                    var times = _log.getSceneTime(k, sid);
                    if (times == null) continue;
                    timeSec[i][k] = TimeSpan.FromSeconds(0);
                    if (times[0] != null && times[1] != null)
                        timeSec[i][k] = _log.timeDifferenceTimespan(times[0], times[1]);
                    else if (times[0].Length > 0)
                    {
                        //string abortTime = _log.getAbortTime(sid, k);
                        //if (abortTime.Length > 0)
                        //    timeSec[i][k] = _log.timeDifferenceTimespan(times[0], abortTime);
                        //else
                        timeSec[i][k] = _log.timeDifferenceTimespan(times[0], times[0]); ;
                    }
                    var xyzTable = _log.GetPath(sid, k);
                    distances[i][k] = MenuUtils.ComputeParticipantPathDistance(xyzTable);
                }

                var pid = participant_ids[i];

                var gObject = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/EvaluationEntry");
                gObject.transform.Find("DetailsButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowParticipantDetails(sid);

                });
                gObject.transform.Find("RemoveButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    RemoveEvaluationEntry(sid, pid, gObject);

                });
                MenuUtils.PlaceElement(gObject, _dynFields);

                gObject.transform.Find("SessionId").GetComponent<Text>().text = sid.ToString();
                gObject.transform.Find("ParticipantId").GetComponent<Text>().text = pid;
            }

            //creates a map on the second camera
            _map.GetComponent<PopUpEvaluationMap>().SetupMaps(envs);
        }

        /// <summary>
        /// Opens participant details menu
        /// </summary>
        /// <param name="sid">Session Id of the participant</param>
        public void ShowParticipantDetails(int sid)
        {
            _menuManager.ActiveSessionId = sid;
            _menuManager.InstantiateAndShowMenu("Participant Menu","Launcher");
        }

        /// <summary>
        /// Open the Confirm Deletion Menu to ensure that a data point should be deleted.
        /// </summary>
        /// <param name="sid">Participant's session Id</param>
        /// <param name="pid">Participant's id.</param>
        /// <param name="item">List item that will be removed upon confirmation</param>
        public void RemoveEvaluationEntry(int sid, string pid, GameObject item)
        {
            _menuManager.ActiveSessionId = sid;
            _menuManager.ActiveParticipantId = pid;
            _menuManager.InstantiateAndShowMenu("Delete Participant Menu", "Launcher");
        }
    }
}
