using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;

public class ShowSkinConductance : MonoBehaviour {

	public GameObject flashingObjectCh1;
	public GameObject flashingObjectCh2;
	public Camera cam;

	public float ch1dist = 10;
	public float ch1right = 1.5f;
	public float ch1top = 3.7f;
	public float ch2dist = 10;
	public float ch2right = -1.5f;
	public float ch2top = 3.7f;

	private string text;
	private float minCh1 = 0;
	private float maxCh1 = 0.000001f;	//dont divide by zero :)
	private float minCh2 = 0;
	private float maxCh2 = 0.000001f;

	private bool setToZeroCh1 = false;
	private bool setToZeroCh2 = false;


	void Start () {
		//open .exe to read stream from Labchart
		string path = "";
        
        LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        LoggingManager log = launchManager.LoggingManager;
        if (log == null) 	path = @"D:/CityExperiment/LabChartFiles/002.adicht";
		else 				path = @log.GetLabChartFilePath (); 

		Process foo = new Process();
		foo.StartInfo.FileName = "D:/CityExperiment/LabChartFiles/ProgramStreamData/bin/Debug/DriveChart.exe";
		foo.StartInfo.Arguments = path;
		//foo.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		foo.Start();
		if(log != null) log.RecordLabChartStartTime();
	}

	void Update () {
		flashingObjectCh1.transform.position = cam.transform.position + cam.transform.forward * ch1dist + cam.transform.right * ch1right + cam.transform.up * ch1top;
		flashingObjectCh2.transform.position = cam.transform.position + cam.transform.forward * ch2dist + cam.transform.right * ch2right + cam.transform.up * ch2top;

		//open txt file safely, such that no other process can acess it
		try {
			text = File.ReadAllText ("D:/CityExperiment/LabChartFiles/StreamingValues.txt");
		} catch {}
	}

	void OnGUI() {
		//GUI.Label (new Rect(100, 100, 400, 30), text );

		string[] stringSeparators = new string[] {"\r\n"};
		string[] splitText = text.Split ( stringSeparators , StringSplitOptions.None );

		float valueCh1 = 0;
		float valueCh2 = 0;
		float.TryParse(splitText[0], out valueCh1);	// can be somewhat in between -0.5 and 20 -> sometimes more!!!
		float.TryParse(splitText[1], out valueCh2);	// can be somewhat in between -0.5 and 20 -> sometimes more!!!

		//set limits of intensity
		if (!setToZeroCh1) {
			minCh1 = valueCh1;
			maxCh1 = valueCh1 + 0.000001f;
			setToZeroCh1 = true;
		}
		if (!setToZeroCh2) {
			minCh2 = valueCh2;
			maxCh2 = valueCh2 + 0.000001f;
			setToZeroCh2 = true;
		}

		if (valueCh1 < minCh1) minCh1 = valueCh1;
		if (valueCh1 > maxCh1) maxCh1 = valueCh1;

		if (valueCh2 < minCh2) minCh2 = valueCh2;
		if (valueCh2 > maxCh2) maxCh2 = valueCh2;

		float intensity1 = (valueCh1 - minCh1) / maxCh1; // map to interval 0-1
		float intensity2 = (valueCh2 - minCh2) / maxCh2; // map to interval 0-1

		float r = intensity1;
		float g = intensity2;
		flashingObjectCh1.transform.GetComponent<Renderer>().material.color = new Color(r,0,0,1);
		flashingObjectCh2.transform.GetComponent<Renderer>().material.color = new Color(0,g,0,1);
	}
}




/*
		try
		{
			//Create an object to read data from the file
			//ADIDataReader reader = new ADIDataReader("../../../../DataFiles/MultirateData.adicht")
			//ADIDataReader reader = new ADIDataReader("C:\\Users\\Katja\\Dropbox\\Job Experiment Interface\\LabChartFiles\\001.adidat");
			ADIDataReader reader = new ADIDataReader("D:\\CityExperiment\\LabChartFiles\\002.adicht");
			ADIDatRead.ShowChannelStats.ShowStats(reader);

		}
		catch (System.Exception ex) {
			Debug.Log("Unexpected exception: " + ex.Message);
		}
		*/
