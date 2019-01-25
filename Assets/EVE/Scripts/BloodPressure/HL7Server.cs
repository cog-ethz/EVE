using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Threading;
using System.Globalization;

class HL7Server
{
    //private static readonly byte[] Localhost = {129, 132, 201, 204};
   // private static readonly byte[] Localhost = { 129, 132, 201, 203 };
    private static byte[] Localhost = new byte[4];
    private const int Port = 4000;
	private LoggingManager log;
	private Socket listener;
	private DateTime lastMeasurement;

	// Volatile is used as hint to the compiler that this data
	// member will be accessed by multiple threads.
	private volatile bool _shouldStop;

	public HL7Server(LoggingManager logger, String ipaddress) {
		log = logger;
        string[] ipSplit = ipaddress.Split('.');
        Localhost[0] = byte.Parse(ipSplit[0]);
        Localhost[1] = byte.Parse(ipSplit[1]);
        Localhost[2] = byte.Parse(ipSplit[2]);
        Localhost[3] = byte.Parse(ipSplit[3]);

	}

	public void RequestStop()
	{
		_shouldStop = true;
		listener.Close ();
	}

	public void DoWork() {
		var address = new IPAddress (Localhost);
		var endPoint = new IPEndPoint (address, Port);

		listener = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try {
			listener.Bind (endPoint);
			listener.Listen (3);
			while (!_shouldStop) {
				Debug.Log("Listening on port "+ Port);
				String data = "";
				byte[] buffer = new byte[4096];
				// handle incoming connection ...
				var handler = listener.Accept();
				Debug.Log ("Handling incoming connection ...");
				while (true) {
					int count = handler.Receive (buffer);
					data += Encoding.UTF8.GetString (buffer, 0, count);
					// Find start of MLLP frame, a VT character ...
					int start = data.IndexOf ((char)0x0B);
					if (start >= 0) {
						// Now look for the end of the frame, a FS character
						int end = data.IndexOf ((char)0x1C);
						if (end > start) {
							string temp = data.Substring (start + 1, end - start);
                            // handle message
                            string response = HandleMessage (temp);
							// Send response
							handler.Send (Encoding.UTF8.GetBytes (response));
							break;
						}
					}
				}
				// close connection
				handler.Shutdown (SocketShutdown.Both);
				handler.Close ();
				//Console.WriteLine ("Connection closed.");
			}
		} catch (Exception e) {
			Debug.LogError(e.Message);
		}
	}

    private string HandleMessage(string data)
    {
        Message msg = new Message();
        msg.Parse(data);
        // get first PID segment
        var obxField = msg.FindSegment("OBX");
        /*	string NBPs = "";
            string NBPd = "";
            string NBPm = "";
            string Sp02 = "";
            string HR_pulse = "";
            string NBPsUnit = "";
            string NBPdUnit = "";
            string NBPmUnit = "";
            string Sp02Unit = "";
            string HR_pulseUnit = "";
            string time = "";
            while (obxField != null)
            {
                if (obxField.getCount() > 10) {
                    // Blood pressure measurments and puls measurements have a time field
                    string name = obxField.Field(4);
                    string value = obxField.Field(7);
                    string unit = obxField.Field(9);
                    string tmptime = obxField.Field(18);

                    switch (name)
                    {
                    case "NBPs":
                        NBPs = value;
                        NBPsUnit = unit;
                        time = tmptime;
                        break;
                    case "NBPd":
                        NBPd = value;
                        NBPdUnit = unit;
                        break;
                    case "NBPm":
                        NBPm = value;
                        NBPmUnit = unit;
                        break;
                    case "HR_Pulse":
                        HR_pulse = value;
                        HR_pulseUnit = unit;
                        break;
                    }

                     lastMeasurement = DateTime.ParseExact(time, "yyyyMMddHHmmss", new CultureInfo("de-DE"));
                    //String tmp =  ""+obxField.Field(4)+" "+ obxField.Field(7)+" "+ obxField.Field(9)+" "+ obxField.Field(18);
                    //Debug.Log(tmp);
                }
                else {
                    // SP02 has no time field
                    //string name = obxField.Field(4);
                    string value = obxField.Field(7);
                    string unit = obxField.Field(9);

                    Sp02 = value;
                    Sp02Unit = unit;
                    //string tmp =  ""+obxField.Field(4)+" "+ obxField.Field(7)+" "+ obxField.Field(9);
                    //Debug.Log(tmp);

                }
                obxField = msg.FindNextSegment("OBX", obxField);
            }
            log.insertBPMeasurement (NBPs,NBPd,NBPm,Sp02,HR_pulse,NBPsUnit ,NBPdUnit,NBPmUnit,Sp02Unit,HR_pulseUnit,time);*/
        string time = "";
        while (obxField != null)
        {
            if (obxField.getCount() > 10) {
				// Blood pressure measurments and puls measurements have a time field
				string name = obxField.Field(4);
				string value = obxField.Field(7);
				string unit = obxField.Field(9);
                Debug.Log(name + " " + value + " " + unit);
				time = obxField.Field(18);
                log.InsertMeasurement("HL7Server", name,unit,value, time);
                lastMeasurement = DateTime.ParseExact(time, "yyyyMMddHHmmss", new CultureInfo("de-DE"));
            }
            else
            {
                // SP02 has no time field
                string name = obxField.Field(4);
                string value = obxField.Field(7);
                string unit = obxField.Field(9);
                log.InsertMeasurement("HL7Server", name, unit, value, time);
            }
            obxField = msg.FindNextSegment("OBX", obxField);

        }

        // Create a response message
        var response = new Message();
        var msh = new Segment("MSH");
        msh.Field(2, "^~\\&");
        msh.Field(7, DateTime.Now.ToString("yyyyMMddhhmmsszzz"));
        msh.Field(9, "ACK");
        msh.Field(10, Guid.NewGuid().ToString() );
        msh.Field(11, "P");
        msh.Field(12, "2.4");
        response.Add(msh);

        var msa = new Segment("MSA");
        msa.Field(1, "AA");
        msa.Field(2, msg.MessageControlId());
        response.Add(msa);
        
        // Put response message into an MLLP frame ( <CR> data )
        var frame = new StringBuilder();
        frame.Append((char) 0x0D);
        frame.Append(response.Serialize());
        return frame.ToString();
    }

	public DateTime getLastMeasurmentTime() {
		return lastMeasurement;
	}
}
