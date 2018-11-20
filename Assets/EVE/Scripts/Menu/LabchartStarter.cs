using UnityEngine;
using System.Diagnostics;
using Assets.EVE.Scripts.Menu;
using UnityEngine.SceneManagement;

public class LabchartStarter : MonoBehaviour {

    private LoggingManager _log;
    private string _starterPath;
    private string _path;
    private LaunchManager _launchManager;

    // Use this for initialization
	void Start ()
	{
	    _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
	    _path = _launchManager.ExperimentSettings.LabchartSettings.Path;
        _starterPath = _path + "StartData\\DriveChart.exe";
        _log = _launchManager.LoggingManager;
    }

    public void StartLabchartMeasuring()
    {
        StartLabChart();
        _launchManager.MenuManager.CloseCurrentMenu();
        _launchManager.ManualContinueToNextScene();
        //SceneManager.LoadScene("Loader");
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
