using UnityEngine;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class LabchartStarter : MonoBehaviour {

    private LoggingManager _log;
    private string _starterPath;
    private string _participantsPath;

	// Use this for initialization
	void Start ()
	{
	    var launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        _starterPath = launchManager.ExperimentSettings.LabchartSettings.StarterPath;
        _participantsPath = launchManager.ExperimentSettings.LabchartSettings.ParticipantsPath;
        _log = launchManager.GetLoggingManager();
    }

    public void StartLabchartMeasuring()
    {
        StartLabChart();
        SceneManager.LoadScene("Loader");
    }

    /// <summary>
    /// Calls Labchart to record onto the specified file.
    /// </summary>
    private void StartLabChart()
    {
        string fileName = _participantsPath + _log.GetLabChartFileName() + ".adicht";
		try
        {
            Process foo = new Process();
            foo.StartInfo.FileName = _starterPath;
            foo.StartInfo.Arguments = fileName;
            foo.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            foo.Start();
        }
        catch
        {
            UnityEngine.Debug.LogWarning("Labchart not found at path: " + _starterPath);
        }
        if (_log != null) _log.RecordLabChartStartTime();
    }
}
