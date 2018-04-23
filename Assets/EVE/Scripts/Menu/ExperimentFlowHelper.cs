using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentFlowHelper : MonoBehaviour
{
    private LaunchManager _launchManager;

    void Awake()
    {
        _launchManager = GameObject.FindWithTag("LaunchManager").GetComponent<LaunchManager>();
    }

	public void findAndStart()
    {
        _launchManager.startExperiment();
    }

    public void findAndStop()
    {
        _launchManager.setCompletedAndReset();
    }

    public void findAndLoad()
    {
        _launchManager.loadCurrentScene();
    }
}
