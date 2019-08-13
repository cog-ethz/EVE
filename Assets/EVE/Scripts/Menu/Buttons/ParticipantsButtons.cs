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
        //answer vars
        private LoggingManager _log;

        private int[] session_ids;
        private string[] participant_ids;

        private LaunchManager _launchManager;
        private MenuManager _menuManager;
        private Transform _dynFields;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _log = _launchManager.LoggingManager;

            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => { _menuManager.InstantiateAndShowMenu("Evaluation Menu", "Launcher"); });

            _dynFields = fields.Find("DynFieldsWithScrollbar").Find("DynFields");

            DisplayParticipants();
        }

        public void DisplayParticipants()
        {
            MenuUtils.ClearList(_dynFields);

            var experimentName = _launchManager.ExperimentName;
            var s = _log.GetAllSessionsData(experimentName);
            session_ids = Array.ConvertAll(s[0], int.Parse);
            participant_ids = s[1];
            
            for (var i = 0; i < session_ids.Length; i++)
            {
                var sid = session_ids[i];
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
            //_map.GetComponent<PopUpEvaluationMap>().SetupMaps(envs);
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
