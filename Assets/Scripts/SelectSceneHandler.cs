using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using LitJson;

public class SelectSceneHandler : MonoBehaviour {
	public GameObject itemGroup;
	public GameObject dialogPanel;

	private Image[] items = new Image[3];
	private Button m_create;
	private Button m_Ok, m_cancel;
	private Text m_headerText, m_dialogText;
	private SocketConnector client;
	private Player player;

	// Use this for initialization
	void Start () {
		HideDialog ();
		client = SocketConnector.GetInstance ();
		player = Player.GetInstance ();
		InitItems ();
		UpdateItems ();
	}

	private delegate int AsyncMethodCaller();

	private int Respone(){
		while (true) {
			if (client.messages.Count > 0) {
				JsonData data = client.messages [0];
				client.messages.RemoveAt (0);
				string msgname = (string)data ["msgname"];
				if ("Respone".Equals(msgname)) {
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

	private int AcquireRoles(){
		while (true) {
			if (client.messages.Count > 0) {
				JsonData data = client.messages [0];
				client.messages.RemoveAt (0);
				string msgname = (string)data ["msgname"];
				if ("Roles".Equals(msgname)){
					player.SetRoles (data ["roles"]);
					return 0;
				}
			}
		}
	}
		
	public void CreateRole() {
		SceneManager.LoadScene("create");
	}

	public void DeleteRole() {
		ShowDialog("删除角色", "确定要删除角色吗？", true);
		string data = Processor.C2SDeleteRole (player.userId, player.roles [player.selectedRole].id);
		if (!client.IsConnected()) {
			if (!client.Connect ("127.0.0.1", 8888)) {
				Debug.LogError ("Connected failed!");
				ShowDialog ("删除角色", "无法连接到服务器！", true, () => {
					HideDialog ();
				}, () => {
					HideDialog ();
				});
				return;
			}
		}
		ShowDialog("删除角色", "处理中...", false);
		client.Send (data);
		AsyncMethodCaller caller = new AsyncMethodCaller(Respone);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (5000, true);
		if (!success) {
			Debug.Log ("Time Out");
			ShowDialog ("删除角色", "操作超时", true, () => {
				HideDialog ();
			}, () => {
				HideDialog ();
			});
		} else {
			int returnValue = caller.EndInvoke(result);
			if (returnValue != 0) {
				success = false;
				ShowDialog ("删除角色", "操作失败", true, () => {
					HideDialog ();
				}, () => {
					HideDialog ();
				});
			}
		}
		result.AsyncWaitHandle.Close();
		if (success) {
			Debug.Log ("Success!");
			ShowDialog ("删除角色", "操作成功！", true, () => {
				HideDialog ();
			}, () => {
				HideDialog ();
			});
			UpdateItems ();
		}
	}

	private void InitItems(){
		Image[] images = itemGroup.GetComponentsInChildren<Image> ();
		int i = 0;
		foreach (Image image in images) {
			if ("Item".Equals (image.name)) {
				Button[] buttons = image.GetComponentsInChildren<Button> ();
				foreach (Button b in buttons) {
					switch (b.name) {
					case "btn_select":
						if (b.onClick.GetPersistentEventCount() == 0) {
							b.onClick.AddListener (() => {
								player.selectedRole = i;
								SceneManager.LoadScene ("game");
							});
						}
						break;
					case "btn_delete":
						if (b.onClick.GetPersistentEventCount() == 0) {
							b.onClick.AddListener (() => {
								player.selectedRole = i;
								DeleteRole ();
							});
						}
						break;							
					}
				}
				items [i++] = image;
			}
		}
		Button[] btns = itemGroup.GetComponentsInChildren<Button> ();
		foreach (Button b in btns) {
			if ("btn_create".Equals (b.name))
				m_create = b;
		}
		m_create.onClick.AddListener (CreateRole);
	}

	private void UpdateItems() {
		AsyncMethodCaller caller = new AsyncMethodCaller(AcquireRoles);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (5000, true);
		if (!success) {
			Debug.Log ("Time Out");
			ShowDialog ("获取角色", "获取已创建角色失败！", true, () => {
				client.Disconnect();
				SceneManager.LoadScene("start");
			}, () => {
				client.Disconnect();
				SceneManager.LoadScene("start");
			});
		} else {
			List<Role> roles = player.roles;
			for (int i = 0; i < 3; ++i) {
				Image item = items[i];
				if (i < roles.Count) {
					Text[] texts = item.GetComponentsInChildren<Text> ();
					string name = roles [i].name;
					string description = string.Format("level:{0,4}   HP:{1}\narmor:{2,3}   weapon:{3}\nattack:{4,3}   ammunition:{5}",roles [i].level, roles [i].blood, roles [i].armor, roles [i].weapon, roles [i].attack, roles [i].ammunition);
					foreach (Text t in texts) {
						switch (t.name) {
						case "name":
							t.text = name;
							break;
						case "description":
							t.text = description;
							break;
						}
					}
					item.gameObject.SetActive (true);
				}
				else
					item.gameObject.SetActive (false);
			}
		}
		result.AsyncWaitHandle.Close();
	} 

	private void ShowDialog(string headerText, string dialogText, bool showButton = false, UnityAction OKListener = null, UnityAction cancelListener = null){
		dialogPanel.SetActive (true);
		if (m_headerText == null || m_dialogText == null) {
			Text[] texts = dialogPanel.GetComponentsInChildren<Text> ();
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

		if (m_Ok == null || m_cancel == null) {
			Button[] buttons = dialogPanel.GetComponentsInChildren<Button> ();
			foreach (Button b in buttons) {
				switch (b.name) {
				case "Ok":
					m_Ok = b;
					break;
				case "Cancel":
					m_cancel = b;
					break;							
				}
			}
		}
		m_Ok.gameObject.SetActive (false);
		m_cancel.gameObject.SetActive (false);
		if (showButton) {
			m_Ok.gameObject.SetActive (true);
			if (OKListener != null) {
				m_Ok.onClick.RemoveAllListeners ();
				m_Ok.onClick.AddListener (OKListener);
			}
			m_cancel.gameObject.SetActive (true);
			if (cancelListener != null) {
				m_cancel.onClick.RemoveAllListeners ();
				m_cancel.onClick.AddListener (cancelListener);
			}
		}
	}

	private void HideDialog() {
		dialogPanel.SetActive (false);
	}
}
