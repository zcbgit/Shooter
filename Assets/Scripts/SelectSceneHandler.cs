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
	private Player player;

	// Use this for initialization
	void Start () {
		HideDialog ();
		player = Player.GetInstance ();
		InitItems ();
		UpdateItems ();
	}

	private delegate int AsyncMethodCaller();

	private int DeleteRoleRespone(){
		while (true) {
			JsonData data = player.Receive();
			if (data != null) {
				string msgname = (string)data ["msgname"];
				string resmsgname = (string)data ["resmsgname"];
				if ("Respone".Equals(msgname) && "DeleteRole".Equals(resmsgname)) {
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

	private int EnterGameRespone(){
		while (true) {
			JsonData data = player.Receive();
			if (data != null) {
				string msgname = (string)data ["msgname"];
				string resmsgname = (string)data ["resmsgname"];
				if ("Respone".Equals(msgname) && "EnterGame".Equals(resmsgname)) {
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
			JsonData data = player.Receive();
			if (data != null) {
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

	public void EnterGame() {
		player.roleId = player.roles [player.selectedRole].id;
		string data = Processor.C2SEnterGame (player.userId, player.roles [player.selectedRole].id);
		if (!player.IsLogined()) {
			Debug.LogError ("player do not login!");
			ShowDialog ("进入游戏", "与服务器断开或用户未登陆！", true, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			}, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			});
		}
		ShowDialog("进入游戏", "处理中...", false);
		player.Send (data);
		AsyncMethodCaller caller = new AsyncMethodCaller(EnterGameRespone);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (5000, true);
		if (!success) {
			Debug.Log ("Time Out");
			ShowDialog ("进入游戏", "操作超时", true, () => {
				HideDialog ();
			}, () => {
				HideDialog ();
			});
		} else {
			int returnValue = caller.EndInvoke(result);
			if (returnValue != 0) {
				success = false;
				ShowDialog ("进入游戏", "操作失败", true, () => {
					HideDialog ();
				}, () => {
					HideDialog ();
				});
			}
		}
		result.AsyncWaitHandle.Close();
		if (success) {
			SceneManager.LoadScene ("game");
		}
	}

	public void DeleteRole() {
		ShowDialog("删除角色", "确定要删除角色吗？", true);
		string data = Processor.C2SDeleteRole (player.userId, player.roles [player.selectedRole].id);
		if (!player.IsLogined()) {
			Debug.LogError ("player do not login!");
			ShowDialog ("删除角色", "与服务器断开或用户未登陆！", true, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			}, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			});
		}
		ShowDialog("删除角色", "处理中...", false);
		player.Send (data);
		AsyncMethodCaller caller = new AsyncMethodCaller(DeleteRoleRespone);
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
				UpdateItems ();
			}, () => {
				HideDialog ();
				UpdateItems ();
			});
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
							switch (i) {
							case 0:
								b.onClick.AddListener (() => {
									player.selectedRole = 0;
									EnterGame();
								});
								break;
							case 1:
								b.onClick.AddListener (() => {
									player.selectedRole = 1;
									EnterGame();
								});
								break;
							case 2:
								b.onClick.AddListener (() => {
									player.selectedRole = 2;
									EnterGame();
								});
								break;
							}
						}
						break;
					case "btn_delete":
						if (b.onClick.GetPersistentEventCount() == 0) {
							switch (i) {
							case 0:
								b.onClick.AddListener (() => {
									player.selectedRole = 0;
									DeleteRole ();
								});
								break;
							case 1:
								b.onClick.AddListener (() => {
									player.selectedRole = 1;
									DeleteRole ();
								});
								break;
							case 2:
								b.onClick.AddListener (() => {
									player.selectedRole = 2;
									DeleteRole ();
								});
								break;
							}
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
		if (!player.IsLogined()) {
			Debug.LogError ("player do not login!");
			ShowDialog ("获取角色", "与服务器断开或用户未登陆！", true, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			}, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			});
		}
		ShowDialog("获取角色", "处理中...", false);
		string data = Processor.C2SGetRoles (player.userId);
		player.Send (data);
		AsyncMethodCaller caller = new AsyncMethodCaller(AcquireRoles);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (5000, true);
		if (!success) {
			Debug.Log ("Time Out");
			ShowDialog ("获取角色", "获取已创建角色失败！", true, () => {
				player.Logout();
				SceneManager.LoadScene("start");
			}, () => {
				player.Logout();
				SceneManager.LoadScene("start");
			});
		} else {
			List<Role> roles = player.roles;
			for (int i = 0; i < 3; ++i) {
				Image item = items[i];
				if (i < roles.Count) {
					Text[] texts = item.GetComponentsInChildren<Text> ();
					string name = roles [i].name;
					string description = string.Format("level:{0,4}   HP:{1}\narmor:{2,3}   weapon:{3}\nattack:{4,3}   ammunition:{5}",roles [i].level, roles [i].maxHP, roles [i].exp, roles [i].weapon, roles [i].attack, roles [i].ammunition);
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
		HideDialog ();
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
