using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class EvaluationLabchart : MonoBehaviour {
    
    private LoggingManager _log;
    private string _commentWriterPath;
    private string _participantsPath;
    private string _experimentName;
    private List<string> _commenters;

    void Start()
    {
        var launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = launchManager.GetLoggingManager();
        _commentWriterPath = launchManager.ExperimentSettings.LabchartSettings.CommentWriterPath;
        _participantsPath = launchManager.ExperimentSettings.LabchartSettings.ParticipantsPath;
        _experimentName = launchManager.ExperimentSettings.Name;
        _commenters = launchManager.ExperimentSettings.LabchartSettings.Commenters;
    }

    /// <summary>
    /// Adds the comments from the Labchart sensor to the last participant that was run.
    /// </summary>
    /// <remarks>
    /// Note this explicitly adds data to the sessionID-1.
    /// </remarks>
    public void addLabchartComments()
    {
		var sessionId = _log.GetCurrentSessionID()-1;

        var file = _log.getSessionData(sessionId)[3];

        AddLabchartComments(sessionId, file);
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

    public void AddLabchartCommentsToAll()
    {
        var sessionData = _log.getAllSessionsData(_experimentName);

        for (var i = 0; i < sessionData[0].Length; i++)
        { 
            var sessionId = int.Parse(sessionData[0][i]);
            var file = sessionData[3][i];

            AddLabchartComments(sessionId, file);
        }
    }

    private void AddSensorToLabChart(string sensorName, int sessionId, string file)
    {
        var events = _log.getSessionMeasurmentsAsString(sensorName, sessionId);
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
        var sceneNames = _log.getListOfEnvironments(sessionId);
        var nScenes = sceneNames.Length;
        for (var k = 0; k < nScenes; k++)
        {
            var sceneTime = _log.getSceneTime(k, sessionId);
            if (sceneTime != null)
            {
                AddCommentToLabChart(file, "Scene " + sceneNames[k] + k + " start", sceneTime[0], sessionId);
                if (sceneTime[1].Length > 0)
                    AddCommentToLabChart(file, "End of scene " + sceneNames[k] + k, sceneTime[1], sessionId);
                else
                {
                    // Needs to be fixed, currently gets time from store postitions (which only works for virtual environments
                    var abortTime = _log.getAbortTime(sessionId, k);
                    if (abortTime.Length > 0)
                        AddCommentToLabChart(file, "End of scene " + sceneNames[k] + k + "-stopped Manually", abortTime, sessionId);
                }
            }
        }
    }

    private void AddCommentToLabChart(string fileName, string comment, string timestamp, int session)
    {
		var filePath = _participantsPath + fileName + ".adicht";
        
        var labchartStart = _log.getLabchartStarttime(session);
        var ms = (int)_log.timeDifference(labchartStart, timestamp) / 1000;
        
		var args = filePath + " \"" + comment + "\" " + ms;
        
        var p = new Process();
        var psi = new ProcessStartInfo
        {
            FileName = _commentWriterPath,
            Arguments = args
        };
        p = Process.Start(psi);
        p.WaitForExit();
    }
}
