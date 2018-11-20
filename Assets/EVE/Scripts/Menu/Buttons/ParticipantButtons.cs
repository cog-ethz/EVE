using System;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
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

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menumanager = _launchManager.GetMenuManager();
            _log = _launchManager.GetLoggingManager();
            
            _dynamicField = transform.Find("Panel").Find("Fields").Find("DynFieldsWithScrollbar").Find("DynFields");

            DisplayParticipantDetails();
        }

        /// <summary>
        /// Displays all summary information that EVE currently knows.
        /// </summary>
        public void DisplayParticipantDetails()
        {
            Utils.MenuUtils.ClearList(_dynamicField);

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
                    var replayButton = filenameObj.transform.Find("Button").GetComponent<ReplayButton>();
                    replayButton.setreplaySceneID(k);
                    replayButton.setReplaySceneName(envs[k]);
                    replayButton.setReplaySessionId(_sessionId);

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
            Utils.MenuUtils.PlaceElement(filenameObj, dynamicField);
            filenameObj.transform.Find("evalField").GetComponent<Text>().text = evalField;
            filenameObj.transform.Find("evalValue").GetComponent<Text>().text = evalValue;
        }
    }
}
