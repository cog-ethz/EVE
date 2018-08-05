using UnityEngine;
using System.Diagnostics;

public class EvaluationLabchart : MonoBehaviour {
    
    private LoggingManager _log;
    private string _commentWriterPath;
    private string _participantsPath;
    private string _experimentName;

    void Start()
    {
        var launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = launchManager.GetLoggingManager();
        _commentWriterPath = launchManager.ExperimentSettings.LabchartSettings.CommentWriterPath;
        _participantsPath = launchManager.ExperimentSettings.LabchartSettings.ParticipantsPath;
        _experimentName = launchManager.ExperimentSettings.Name;
    }

    /// <summary>
    /// Adds the comments from the Labchart sensor to the last participant that was run.
    /// </summary>
    /// <remarks>
    /// Note this explicitly adds data to the sessionID-1.
    /// </remarks>
    public void addLabchartComments()
    {
		var sessionID = _log.GetCurrentSessionID()-1;

        var file = _log.getSessionData(sessionID)[3];

        AddLabchartComments(sessionID, file);
    }

    public void AddLabchartComments(int sessionID, string file)
    {
        if (file.Length > 0)
        {
            AddScenesToLabChart(sessionID, file);

            AddSensorToLabChart("Labchart", sessionID, file);
            AddSensorToLabChart("QuestionnaireSystem", sessionID, file);

        }
        else
        {
            UnityEngine.Debug.Log("Labchart file is not recorded");
        }
    }

    public void AddLabchartCommentsToAll()
    {
        var sessionData = _log.getAllSessionsData(_experimentName);

        for (int i = 0; i < sessionData[0].Length; i++)
        { 
            int sessionID = int.Parse(sessionData[0][i]);
            var file = sessionData[3][i];

            AddLabchartComments(sessionID, file);
        }
    }

    private void AddSensorToLabChart(string sensorName, int sessionID, string _file)
    {
        var events = _log.getSessionMeasurmentsAsString("Labchart", sessionID);
        if (events != null)
        {
            for (var j = 0; j < events[0].Count; j++)
            {
                AddCommentToLabChart(_file, events[1][j], events[0][j], sessionID);
            }
        }
    }

    private void AddScenesToLabChart(int sessionID, string _file)
    {
        //add scene start and end
        var sceneNames = _log.getListOfEnvironments(sessionID);
        var nScenes = sceneNames.Length;
        for (var k = 0; k < nScenes; k++)
        {
            var sceneTime = _log.getSceneTime(k, sessionID);
            if (sceneTime != null)
            {
                AddCommentToLabChart(_file, "Scene " + sceneNames[k] + k + " start", sceneTime[0], sessionID);
                if (sceneTime[1].Length > 0)
                    AddCommentToLabChart(_file, "End of scene " + sceneNames[k] + k, sceneTime[1], sessionID);
                else
                {
                    // Needs to be fixed, currently gets time from store postitions (which only works for virtual environments
                    var abortTime = _log.getAbortTime(sessionID, k);
                    if (abortTime.Length > 0)
                        AddCommentToLabChart(_file, "End of scene " + sceneNames[k] + k + "-stopped Manually", abortTime, sessionID);
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
