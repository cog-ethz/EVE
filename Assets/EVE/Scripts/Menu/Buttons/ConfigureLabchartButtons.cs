using System.Diagnostics;
using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ConfigureLabchartButtons : MonoBehaviour {

        private LoggingManager _log;
        private string _starterPath;
        private string _path;

        // Use this for initialization
        void Start()
        {
            var launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
            _path = launchManager.ExperimentSettings.LabchartSettings.Path;
            _starterPath = _path + "StartData\\DriveChart.exe";
            _log = launchManager.LoggingManager;
        }

        public void StartLabchartMeasuring()
        {
            StartLabChart();
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
