using UnityEngine;
using System.Diagnostics;

public class EvaluationLabchart : MonoBehaviour {

    private string _file;
    private LoggingManager _log;
    private string _commentWriterPath;
    private string _participantsPath;

    void Start()
    {
        var launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
        _log = launchManager.GetLoggingManager();
        _commentWriterPath = launchManager.ExperimentSettings.LabchartSettings.CommentWriterPath;
        _participantsPath = launchManager.ExperimentSettings.LabchartSettings.ParticipantsPath;
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

        _file = _log.getSessionData(sessionID)[3];

        if (_file.Length > 0)
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
            // add all trigger events
            var events = _log.getSessionMeasurmentsAsString("Labchart", sessionID);
            if (events != null)
            {
                for (var j = 0; j < events[0].Count; j++)
                {
                    AddCommentToLabChart(_file, events[1][j], events[0][j], sessionID);
                }
            }
        } else
        {
            UnityEngine.Debug.Log("Labchart file is not recorded");
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
