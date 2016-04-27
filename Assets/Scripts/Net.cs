using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LitJson;

public class Net: MonoBehaviour {
	private SocketConnector sockectclient;


	// Use this for initialization
	void Start () {
		sockectclient = SocketConnector.GetInstance ();
		sockectclient.Connect ("127.0.0.1", 8888);
	}

	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate()
	{
		while (sockectclient.messages.Count > 0) 
		{
			JsonData data = sockectclient.messages [0];
			sockectclient.messages.RemoveAt (0);
			Processor.Process (data);
		}
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 200, 20), "echo"))
		{
			string msg = Processor.C2SEcho ("test");
			sockectclient.Send(msg);
		}

		if (GUI.Button(new Rect(100, 200, 200, 20), "login"))
		{
			string msg = Processor.C2SLogin ("test", "123");
			sockectclient.Send(msg);
		}

		if (GUI.Button(new Rect(100, 300, 200, 20), "register"))
		{
			string msg = Processor.C2SRegister ("test", "123");
			sockectclient.Send(msg);
		}
	}

}