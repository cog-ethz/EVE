using System;
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

        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menumanager = _launchManager.GetMenuManager();
            _log = _launchManager.GetLoggingManager();

            DisplayParticipantDetails();
        }

        /// <summary>
        /// Displays all summary information that EVE currently knows.
        /// </summary>
        public void DisplayParticipantDetails()
        {
            _sessionId = _menumanager.getDetailsInt();

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

                if (_log.getXYZ(_sessionId, k)[0].Count > 0)
                {
                    var distance = ComputeDistance(_sessionId, k);
                    InstantiateElement("Distance", distance.ToString(),true);

                    //make replay button
                    var filenameObj = Instantiate(Resources.Load("Prefabs/Menus/TabTabAndReplayButton")) as GameObject;
                    var dynamicField = GameObject.Find("Evaluation Details").GetComponent<BaseMenu>().getDynamicFields("DynFieldsA");
                    Utils.PlaceElement(filenameObj, dynamicField);
                    var replayButton = filenameObj.transform.Find("Button").GetComponent<ReplayButton>();
                    replayButton.setreplaySceneID(k);
                    replayButton.setReplaySceneName(envs[k]);
                    replayButton.setReplaySessionId(_sessionId);

                    //make show map button
                    _map = gameObject.GetComponent<PopUpEvaluationMap>();
                    //map.SetupMaps(envNumber);
                    var filenameObj2 = Instantiate(Resources.Load("Prefabs/Menus/TabTabAndShowMapButton")) as GameObject;
                    Utils.PlaceElement(filenameObj2, dynamicField);
                    var showMapButton = filenameObj2.transform.Find("Button").GetComponent<ShowMapButton>();
                    showMapButton.setupButton();
                    showMapButton.setEnvNumber(k);
                    showMapButton.setEnvName(envs[k]);
                    showMapButton.setSessionID(_sessionId);
                    showMapButton.setMap(_map);
                }
            }
        }

        public void InstantiateElement(string evalField, string evalValue,bool tab = false)
        {
            var prefabType = tab ? "TextWithTextAndTab" : "TextWithText";
            var filenameObj = Instantiate(Resources.Load("Prefabs/Menus/" + prefabType)) as GameObject;
            var dynamicField = GameObject.Find("Participant Menu").GetComponent<BaseMenu>().getDynamicFields("DynFields");
            Utils.PlaceElement(filenameObj, dynamicField);
            filenameObj.transform.Find("evalField").GetComponent<Text>().text = evalField;
            filenameObj.transform.Find("evalValue").GetComponent<Text>().text = evalValue;
        }

        private float ComputeDistance(int sessionID, int sceneID)
        {
            float distance = 0;
            var xyzTable = _log.getXYZ(sessionID, sceneID);

            for (var i = 1; i < xyzTable[0].Count; i++)
            {
                var old = new Vector3(xyzTable[0][i - 1], xyzTable[1][i - 1], xyzTable[2][i - 1]);
                var current = new Vector3(xyzTable[0][i], xyzTable[1][i], xyzTable[2][i]);
                distance += (current - old).magnitude;
            }

            return distance;
        }
    }
}
