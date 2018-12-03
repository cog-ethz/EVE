using System;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ParticipantButtons : MonoBehaviour {

        int _sessionId = -1;
        private MenuManager _menumanager;
        private LoggingManager _log;
        private PopUpEvaluationMap _map;
        private LaunchManager _launchManager;
        private Transform _dynamicField;
        private LabchartUtils _labchart;

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menumanager = _launchManager.MenuManager;
            _log = _launchManager.LoggingManager;
            _labchart = _launchManager.gameObject.GetComponent<LabchartUtils>();
            
            _dynamicField = transform.Find("Panel").Find("Fields").Find("DynFieldsWithScrollbar").Find("DynFields");
            var btn = transform.Find("Panel").Find("Fields").Find("Labchart Button").GetComponent<Button>();
            btn.onClick.AddListener(AddLabchartComments);
            DisplayParticipantDetails();
        }

        /// <summary>
        /// Displays all summary information that EVE currently knows.
        /// </summary>
        public void DisplayParticipantDetails()
        {
            MenuUtils.ClearList(_dynamicField);

            _sessionId = _menumanager.ActiveSessionId;

            var envs = _log.getListOfEnvironments(_sessionId);
            var timeSec = new TimeSpan[envs.Length];

            InstantiateElement("Session ID", _sessionId.ToString());
            
            for (var k = 0; k < envs.Length; k++)
            {
                InstantiateElement("Scene", "Scene " + k + ": " + envs[k]);
                timeSec[k] = TimeSpan.FromSeconds(0);
                var times = _log.getSceneTime(k, _sessionId);
                if (times[0] != null && times[1] != null)
                    timeSec[k] = _log.timeDifferenceTimespan(times[0], times[1]);
                else if (times[0].Length > 0)
                {
                    //string abortTime = log.getAbortTime(sessionID, k);
                    //if (abortTime.Length > 0)
                    //    timeSec[k] = log.timeDifferenceTimespan(times[0], abortTime);
                    //else
                    timeSec[k] = _log.timeDifferenceTimespan(times[0], times[0]);
                }
                InstantiateElement("Time", timeSec[k].TotalSeconds.ToString(),true);

                var xyzTable = _log.getXYZ(_sessionId, k);
                if (xyzTable[0].Count > 0)
                {
                    var distance = Utils.MenuUtils.ComputeParticipantPathDistance(xyzTable);
                    InstantiateElement("Distance", distance.ToString(),true);

                    //make replay button
                    var filenameObj = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/TabTabAndReplayButton");
                    MenuUtils.PlaceElement(filenameObj, _dynamicField);
                    var replayButton = filenameObj.transform.Find("Button").GetComponent<Button>();
                    var localSceneId = k;
                    var localSceneName = envs[k];
                    var localSessionId = _sessionId;
                    replayButton.onClick.AddListener(() => Replay(localSceneId, localSceneName, localSessionId));

                    //make show map button
                    _map = gameObject.GetComponent<PopUpEvaluationMap>();
                    //map.SetupMaps(envNumber);
                    var filenameObj2 = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/TabTabAndShowMapButton");
                    MenuUtils.PlaceElement(filenameObj2, _dynamicField);
                    var showMapButton = filenameObj2.transform.Find("Button").GetComponent<ShowMapButton>();
                    showMapButton.setupButton();
                    showMapButton.setEnvNumber(k);
                    showMapButton.setEnvName(envs[k]);
                    showMapButton.setSessionID(_sessionId);
                    showMapButton.setMap(_map);
                }
            }
        }

        public void InstantiateElement(string evalField, string evalValue, bool tab = false)
        {
            var prefabType = tab ? "TextWithTextAndTab" : "TextWithText";
            var filenameObj = GameObjectUtils.InstatiatePrefab("Prefabs/Menus/" + prefabType);
            var dynamicField = GameObject.Find("Participant Menu").GetComponent<BaseMenu>().getDynamicFields("DynFields");
            MenuUtils.PlaceElement(filenameObj, dynamicField);
            filenameObj.transform.Find("evalField").GetComponent<Text>().text = evalField;
            filenameObj.transform.Find("evalValue").GetComponent<Text>().text = evalValue;
        }

        public void AddLabchartComments()
        {
            _labchart.AddLabchartComments();
        }

        public void Replay(int sessionId, string sceneName, int sceneId)
        {
            GameObject FPC = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>().FPC;
            ReplayRoute replay = FPC.GetComponentInChildren<ReplayRoute>();
            replay.activateReplay(sessionId, sceneName, sceneId);
            SceneManager.LoadScene(sceneName);
        }
    }
}
