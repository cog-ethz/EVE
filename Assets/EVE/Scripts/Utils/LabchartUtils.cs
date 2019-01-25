using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Assets.EVE.Scripts.Utils
{
    public class LabchartUtils: MonoBehaviour {
    
        private LoggingManager _log;
        private string _commentWriterPath;
        private string _path;
        private string _experimentName;
        private List<string> _commenters;

        void Start()
        {
            GetLabchartConfiguration();
        }

        /// <summary>
        /// Loads all configuration information from the settings.
        /// </summary>
        public void GetLabchartConfiguration()
        {
            var launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
            _log = launchManager.LoggingManager;
            _path = launchManager.ExperimentSettings.LabchartSettings.Path;
            _commentWriterPath = _path + "AddComments\\DriveChart.exe";
            _experimentName = launchManager.ExperimentSettings.Name;
            _commenters = launchManager.ExperimentSettings.LabchartSettings.Commenters;
        }

        internal void AddLabchartComments(int sessionId)
        {
            var file = _log.GetSessionData(sessionId)[3];

            AddLabchartComments(sessionId, file);
        }

        /// <summary>
        /// Adds the comments from the Labchart sensor to the last participant that was run.
        /// </summary>
        /// <remarks>
        /// Note this explicitly adds data to the sessionID-1.
        /// </remarks>
        public void AddLabchartComments()
        {
            var sessionId = _log.CurrentSessionId-1;

            var file = _log.GetSessionData(sessionId)[3];

            AddLabchartComments(sessionId, file);
        }

        public void AddLabchartCommentsToAll()
        {
            var sessionData = _log.GetAllSessionsData(_experimentName);

            for (var i = 0; i < sessionData[0].Length; i++)
            {
                var sessionId = int.Parse(sessionData[0][i]);
                var file = sessionData[3][i];

                AddLabchartComments(sessionId, file);
            }
        }

        public void AddLabchartComments(int sessionID, string file)
        {
            if (file.Length > 0)
            {
                AddScenesToLabChart(sessionID, file);
                for (var i = 0; i < _commenters.Count; i++)
                {
                    AddSensorToLabChart(_commenters[i], sessionID, file);
                }

            }
            else
            {
                UnityEngine.Debug.Log("Labchart file is not recorded");
            }
        }
        
        private void AddSensorToLabChart(string sensorName, int sessionId, string file)
        {
            var events = _log.GetSessionMeasurmentsAsString(sensorName, sessionId);
            if (events != null)
            {
                for (var j = 0; j < events[0].Count; j++)
                {
                    AddCommentToLabChart(file, events[1][j], events[0][j], sessionId);
                }
            }
        }

        private void AddScenesToLabChart(int sessionId, string file)
        {
            //add scene start and end
            var sceneNames = _log.GetListOfEnvironments(sessionId);
            var nScenes = sceneNames.Length;
            for (var k = 0; k < nScenes; k++)
            {
                var sceneTime = _log.GetSceneTime(k, sessionId);
                if (sceneTime != null)
                {
                    AddCommentToLabChart(file, "Scene " + sceneNames[k] + k + " start", sceneTime[0], sessionId);
                    if (sceneTime[1].Length > 0)
                        AddCommentToLabChart(file, "End of scene " + sceneNames[k] + k, sceneTime[1], sessionId);
                    else
                    {
                        // Needs to be fixed, currently gets time from store postitions (which only works for virtual environments
                        var abortTime = _log.GetAbortTime(sessionId, k);
                        if (abortTime.Length > 0)
                            AddCommentToLabChart(file, "End of scene " + sceneNames[k] + k + "-stopped Manually", abortTime, sessionId);
                    }
                }
            }
        }

        private void AddCommentToLabChart(string fileName, string comment, string timestamp, int session)
        {
            var filePath = _path + fileName + ".adicht";
        
            var labchartStart = _log.GetLabchartStarttime(session);
            var ms = (int)_log.TimeDifference(labchartStart, timestamp) / 1000;
        
            var args = filePath + " \"" + comment + "\" " + ms;

            var psi = new ProcessStartInfo
            {
                FileName = _commentWriterPath,
                Arguments = args
            };
            var p = Process.Start(psi);
            p.WaitForExit();
        }
    }
}
