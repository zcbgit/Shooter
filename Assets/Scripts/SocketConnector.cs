using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;
using LitJson;

// 异步操作的状态类
public class StateObject
{
	public Socket workSocket = null;
	public int buffSize;
	public int count;
	public byte[] buffer;
	public List<byte> data;

	public StateObject(Socket s, List<byte> _data, int _buffSize)
	{
		this.workSocket = s;
		this.buffSize = _buffSize;
		this.buffer = new byte[_buffSize];
		if (_data == null)
			this.data = new List<byte> ();
		else
			this.data = _data;
	}
}

// socket操作类，单例模式
public class SocketConnector
{
	private string ip;
	private int port;
	private const int HEADLEN = 4;
	private Socket clientSocket;
	private List<byte> recvBuff;
	public List<JsonData> messages;

	private static SocketConnector instance =  new SocketConnector ();

	// 单例模式，保证所有与服务器的交互都通过同一个socket进行。
	public static SocketConnector GetInstance ()
	{
		return instance;
	}

	SocketConnector ()
	{
		clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		recvBuff = new List<byte> ();
		messages = new List<JsonData> ();
	}

	public bool IsConnected() {
		return clientSocket.Connected;
	}

	// 连接，10s超时
	public bool Connect(string ip, int port)
	{
		IAsyncResult result = clientSocket.BeginConnect (ip, port, null, null);
		result.AsyncWaitHandle.WaitOne (10000, true);
		if (!result.IsCompleted) {
			Close ();
			Debug.Log ("Connect Time Out");
			clientSocket.EndConnect (result);
			return false;
		} else {
			if (!clientSocket.Connected) {
				Debug.Log ("Connect failed!");
				return false;
			}
			clientSocket.EndConnect (result);
			Debug.Log ("Connected!");
			Receive ();
			return true;
		}
	}

	// 异步接收
	private void Receive ()
	{
		if (clientSocket == null || !clientSocket.Connected) {
			Close ();
			return;
		}

		try {
			StateObject so = new StateObject(clientSocket, recvBuff, 2048);
			clientSocket.BeginReceive(so.buffer, 0, so.buffSize, SocketFlags.None, new AsyncCallback(RecviveCallback), so);

		} catch (Exception e) {
			Debug.Log ("Failed to clientSocket error." + e);
			clientSocket.Close ();
		}
	}

	private void RecviveCallback(IAsyncResult ar)
	{
		StateObject so = (StateObject) ar.AsyncState;
		Socket s = so.workSocket;

		so.count = s.EndReceive(ar);

		if (so.count > 0) {
			for (int i = 0; i < so.count; ++i)
				so.data.Add (so.buffer [i]);
			//Debug.Log (String.Format("receive {0} bytes to server!", so.count));
			SplitPackage (0);
			s.BeginReceive(so.buffer, 0, so.buffSize, SocketFlags.None, new AsyncCallback(RecviveCallback), so);
		}
		else {
			Close ();
		}
		
	}

	// 对接收的消息解包
	private void SplitPackage (int beg)
	{
		byte[] head = new byte[HEADLEN];
		while (true) {
			int index = beg;
			if (recvBuff.Count > beg + HEADLEN) {
				for (int i = 0; i < HEADLEN; ++i)
					head [HEADLEN - 1 - i] = recvBuff [index++];
			} else {
				break;
			}

			uint length = BitConverter.ToUInt32 (head, 0);
			if (length > 0 && recvBuff.Count >= index + length - 1) {
				byte[] data = new byte[length];

				for (int i = 0; i < length; ++i)
					data [i] = recvBuff [index++];
				string msg = Encoding.UTF8.GetString (data);
				JsonData jsonData = JsonMapper.ToObject(msg);
				messages.Add (jsonData);
				recvBuff.RemoveRange (beg, index - beg);
				//Debug.LogFormat("{0}--recv {1} bytes to server!", string.Format("{0:mm:ss:ffff}",DateTime.Now), msg);
				//Debug.Log (String.Format("msg: {0}, recvbuff.count: {1}", msg, recvBuff.Count));
			} else {
				break;
			}
		}
	}

	// 异步发送
	public void Send (string str)
	{
		if (clientSocket == null || !clientSocket.Connected) {
			Close ();
			return;
		}
		//Debug.LogFormat("{0}--Send {1} bytes to server!", string.Format("{0:mm:ss:ffff}",DateTime.Now), str);
		try {
			List<byte> sendBuff = new List<byte> ();
			byte[] msg = Encoding.UTF8.GetBytes (str);
			byte[] len = System.BitConverter.GetBytes (msg.Length);
			for (int i = 0; i < HEADLEN; ++i)
				sendBuff.Add(len [HEADLEN - 1 - i]);

			sendBuff.AddRange (msg);
			//clientSocket.Send(sendBuff.ToArray());
			clientSocket.BeginSend (sendBuff.ToArray(), 0,  sendBuff.Count, SocketFlags.None, new AsyncCallback (sendCallback), clientSocket);
		} catch {
			Debug.Log ("send message error");
		}
	}

	private void sendCallback (IAsyncResult ar)
	{
		Socket s = (Socket)ar.AsyncState;
		int count = s.EndSend (ar);
	}

	public void Close(){
		if (clientSocket != null && clientSocket.Connected) {
			clientSocket.Shutdown (SocketShutdown.Both);
			clientSocket.Disconnect (true);
			clientSocket.Close ();
			clientSocket = null;
		}
	}

}
