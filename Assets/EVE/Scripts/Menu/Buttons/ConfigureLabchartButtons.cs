using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ConfigureLabchartButtons : MonoBehaviour {

        private LaunchManager _launchManager;
        private LoggingManager _log;
        private string _starterPath;
        private string _path;

        // Use this for initialization
        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _path = _launchManager.ExperimentSettings.LabchartSettings.Path;
            _starterPath = _path + "StartData\\DriveChart.exe";
            _log = _launchManager.LoggingManager;

            var btn = transform.Find("Panel").Find("Fields").Find("MeasureButton").GetComponent<Button>();
            btn.onClick.AddListener(StartLabchartMeasuring);
        }

        public void StartLabchartMeasuring()
        {
            StartLabChart();
            _launchManager.MenuManager.CloseCurrentMenu();
            _launchManager.ManualContinueToNextScene();
        }

        /// <summary>
        /// Calls Labchart to record onto the specified file.
        /// </summary>
        private void StartLabChart()
        {
            var fileName = _path + _log.GetLabChartFileName() + ".adicht";
            try
            {
                var foo = new Process
                {
                    StartInfo =
                    {
                        FileName = _starterPath,
                        Arguments = fileName,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                foo.Start();
            }
            catch
            {
                UnityEngine.Debug.LogWarning("Labchart not found at path: " + _starterPath);
            }
            if (_log != null) _log.RecordLabChartStartTime();
        }
    }
}
