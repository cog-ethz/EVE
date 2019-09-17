using System;
using System.Collections.Generic;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using EVE.Scripts.LevelLoader;
using EVE.Scripts.Utils;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ParticipantButtons : MonoBehaviour {

        private int _sessionId = -1;
        private MenuManager _menuManager;
        private LoggingManager _log;
        private PopUpEvaluationMap _map;
        private LaunchManager _launchManager;
        private Transform _dynamicField;
        private LabchartUtils _labchart;
        private Transform _fields;

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _log = _launchManager.LoggingManager;
            _labchart = _launchManager.gameObject.GetComponent<LabchartUtils>();

            _sessionId = _menuManager.ActiveSessionId;

            _fields = transform.Find("Panel").Find("Fields");

            _dynamicField = _fields.Find("DynFieldsWithScrollbar").Find("DynFields");

            _fields.Find("Labchart Button").GetComponent<Button>().onClick.AddListener(() => _labchart.AddLabchartComments(_sessionId));
            _fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Participants Menu","Launcher"));
            DisplayParticipantDetails();
        }

        /// <summary>
        /// Displays all summary information that EVE currently knows.
        /// </summary>
        public void DisplayParticipantDetails()
        {
            MenuUtils.ClearList(_dynamicField);


            var envs = _log.GetSceneNames(_launchManager.ExperimentName);// _log.GetListOfEnvironments(_sessionId).Distinct().ToArray(); ;
            var timeSec = new TimeSpan[envs.Length];


            _fields.Find("SessionInformation").Find("SessionId").GetComponent<Text>().text = _sessionId.ToString();
            
            for (var k = 0; k < envs.Length; k++)
            {
                var sceneDescription = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Lists/SceneEntry");
                MenuUtils.PlaceElement(sceneDescription, _dynamicField);

                sceneDescription.transform.Find("SceneInformation").Find("SceneLabel")
                        .GetComponent<Text>().text = "Scene " + k + ":";
                sceneDescription.transform.Find("SceneInformation").Find("SceneValue")
                        .GetComponent<Text>().text = envs[k].Name;
                timeSec[k] = TimeSpan.FromSeconds(0);
                var times = _log.GetSceneTime(k, _sessionId);
                if (times[0] != null && times[1] != null)
                    timeSec[k] = TimeUtils.TimeSpanDifference(times[0], times[1]);
                else if (times[0].Length > 0)
                {
                    timeSec[k] = TimeUtils.TimeSpanDifference(times[0], times[0]);
                }

                sceneDescription.transform.Find("Statistics").Find("TimeInformation").Find("TimeValue")
                    .GetComponent<Text>().text = timeSec[k].TotalSeconds.ToString();

                var xyzTable = _log.GetPath(_sessionId, k);
                if (xyzTable[0].Count <= 0) continue;
                var distance = MenuUtils.ComputeParticipantPathDistance(xyzTable);
                sceneDescription.transform.Find("Statistics").Find("DistanceInformation").Find("DistanceValue")
                    .GetComponent<Text>().text = distance.ToString();

                //make replay button
                var replayButton = sceneDescription.transform.Find("Buttons").Find("ReplayButton").GetComponent<Button>();
                var localSceneId = k;
                var localSceneName = envs[k];
                var localSessionId = _sessionId;
                replayButton.onClick.AddListener(() => Replay(localSessionId, localSceneId, localSceneName.Name));

                //make show map button
                _map = gameObject.GetComponent<PopUpEvaluationMap>();
                var showMapButton = sceneDescription.transform.Find("Buttons").Find("ShowMapButton").GetComponent<Button>();
                showMapButton.onClick.AddListener(()=>ShowMap(localSessionId, localSceneId, localSceneName.Name));
            }
        }
        
        public void Replay(int sessionId, int sceneId, string sceneName)
        {
            var replay = _launchManager.FirstPersonController.GetComponentInChildren<ReplayRoute>();
            replay.activateReplay(sessionId, sceneName, sceneId);
            SceneManager.LoadScene(sceneName);
        }

        public void ShowMap(int sessionId, int sceneId, string sceneName)
        {
            _map = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/Evaluation/EvaluationMap").GetComponent<PopUpEvaluationMap>();
            _map.SetupMaps(sceneName);
            _map.OpenPopUpMap(_log.GetPath(sessionId, sceneId), sceneName);
        }
    }
}
