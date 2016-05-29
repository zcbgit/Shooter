using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using LitJson;

// 开始界面控制脚本
public class StartSceneHandler : MonoBehaviour {
	public Button m_btnLogin;
	public Button m_btnRigister;
	public InputField m_userid;
	public InputField m_password;
	public GameObject m_dialog;

	private Player player;
	private GameObject m_cancel;
	private Text m_headerText;
	private Text m_dialogText;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		HideDialog ();
		m_btnLogin.onClick.AddListener (TryLogin);
		m_btnRigister.onClick.AddListener (TryRegister);
		player = Player.GetInstance ();
	}
	
	private void TryLogin(){
		if (m_userid == null || m_password == null)
			return;
		string userid = m_userid.text;
		string password = m_password.text;
		if (userid.Length == 0 || password.Length == 0) {
			ShowDialog ("登陆", "用户名或密码不能为空", true);
			return;
		}
		string data = Processor.C2SLogin (userid, password);
		if (!player.IsConnected()) {
			if (!player.Connect ("127.0.0.1", 8888)) {
				Debug.LogError ("Connected failed!");
				ShowDialog ("登陆", "无法连接到服务器！", true);
				return;
			}
		}
		ShowDialog("登陆", "处理中...", false);
		player.Send (data);
		AsyncMethodCaller caller = new AsyncMethodCaller(Respone);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (10000, true);
		if (!success) {
			Debug.Log ("Time Out");
			ShowDialog ("登陆", "登陆超时", true);
		} else {
			int returnValue = caller.EndInvoke(result);
			if (returnValue != 0) {
				success = false;
				ShowDialog ("登陆", "登陆失败", true);
			}
		}
		result.AsyncWaitHandle.Close();
		m_password.text = "";
		if (success) {
			player.Login (userid);
			SceneManager.LoadScene("select");
		}
	}
	private delegate int AsyncMethodCaller();

	private void TryRegister(){
		if (m_userid == null || m_password == null)
			return;
		string userid = m_userid.text;
		string password = m_password.text;
		if (userid.Length == 0 || password.Length == 0) {
			ShowDialog ("注册", "用户名或密码不能为空", true);
			return;
		}
		string data = Processor.C2SRegister (userid, password);
		if (!player.IsConnected()) {
			if (!player.Connect ("127.0.0.1", 8888)) {
				ShowDialog ("注册", "无法连接到服务器！", true);
				return;
			}
		}
		ShowDialog("注册", "处理中...", false);
		player.Send (data);
		AsyncMethodCaller caller = new AsyncMethodCaller(Respone);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (10000, true);
		if (!success) {
			ShowDialog ("注册", "注册超时", true);
		} else {
			int returnValue = caller.EndInvoke(result);
			if (returnValue != 0) {
				success = false;
				ShowDialog ("注册", "注册失败", true);
			}
		}
		result.AsyncWaitHandle.Close();
		m_password.text = "";
		if (success) {
			ShowDialog ("注册", "注册成功！请登录！", true);
		}
	}

	private int Respone(){
		while (true) {
			JsonData data = player.Receive ();
			if (data != null) {
				string msgname = (string)data ["msgname"];
				if ("Respone".Equals (msgname)) {
					int errcode = (int)data ["errcode"];
					string errmsg = (string)data ["errmsg"];
					if (errcode != 0) {
						Debug.Log (string.Format ("errcode[{0}], errmsg[{0}]", errcode, errmsg));
					} else {
						Debug.Log ("Login success!");
					}
					return errcode;
				}
			}
		}
	}

	private void ShowDialog(string headerText, string dialogText, bool showButton){
		m_dialog.SetActive (true);
		if (m_headerText == null || m_dialogText == null) {
			Text[] texts = m_dialog.GetComponentsInChildren<Text> ();
			foreach (Text t in texts) {
				switch (t.name) {
				case "HeaderText":
					m_headerText = t;
					break;
				case "DialogText":
					m_dialogText = t;
					break;
				}
			}
		}
		m_headerText.text = headerText;
		m_dialogText.text = dialogText;
		if (m_cancel == null)
			m_cancel = m_dialog.GetComponentInChildren<Button> ().gameObject;
		m_cancel.SetActive (showButton);
	}

	private void HideDialog() {
		m_dialog.SetActive (false);
	}
}
