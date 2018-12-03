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

        // Use this for initialization
        void Start ()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _labchart = _launchManager.gameObject.GetComponent<LabchartUtils>();

            var btn = transform.Find("Panel").Find("Fields").Find("Labchart Button").GetComponent<Button>();
            btn.onClick.AddListener(AddLabchartCommentsToAll);
        }
        
        public void AddLabchartCommentsToAll()
        {
            _labchart.AddLabchartCommentsToAll();
        }
    }
}
