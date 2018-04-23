using System;
using System.Threading;
using UnityEngine;
using System.Net;
using System.Net.Sockets;


public class HL7ServerStarter : MonoBehaviour {
	private Thread workerThread;
	private HL7Server workerObject;
	private Boolean started = false;

    public int minutes = 3;

    private TimeSpan intervall;

	public void Start() {
        intervall = new TimeSpan(0, minutes, 0);
	}

	public void Update() {
		if (!started) {
            LaunchManager launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>(); 
            LoggingManager log = launchManager.GetLoggingManager();

            if (log != null) {
                workerObject = new HL7Server(log, LocalIPAddress());
				workerThread = new Thread (workerObject.DoWork);
				// Start the worker thread.
				workerThread.Start();
				started = true;
			}
		}

	}
	public DateTime getLastMeasurmentTime() {
		if (workerObject != null)
			return workerObject.getLastMeasurmentTime ();
		return DateTime.Now;
	}

	public TimeSpan getIntervall() {
		if (started)
			return intervall;
		else
			return new TimeSpan (0, 0, 0);
	}

	public void OnDestroy() {
			
		if (workerThread != null) {
			workerThread.Abort ();
			workerObject.RequestStop ();
		}
		
	}

    public string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
		Debug.Log("HL7 Server IP:" +localIP);
        return localIP;
    }

}