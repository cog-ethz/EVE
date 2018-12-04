using System.Collections.Generic;
using System.Diagnostics;
using Assets.EVE.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class EvaluationButtons : MonoBehaviour {

        private LabchartUtils _labchart;
        private LaunchManager _launchManager;
        private MenuManager _menuManager;

        // Use this for initialization
        void Start ()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _menuManager = _launchManager.MenuManager;
            _labchart = _launchManager.gameObject.GetComponent<LabchartUtils>();

            var fields = transform.Find("Panel").Find("Fields");
            fields.Find("Participants Button").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Participants Menu", "Launcher"));
            fields.Find("Labchart Button").GetComponent<Button>().onClick.AddListener(() => _labchart.AddLabchartCommentsToAll());
            fields.Find("Back Button").GetComponent<Button>().onClick.AddListener(() => _menuManager.InstantiateAndShowMenu("Main Menu", "Launcher"));
        }
    }
}
