using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using LitJson;

// 角色创建界面脚本
public class CreateSceneHandler : MonoBehaviour {
	public GameObject canvas;
	public GameObject dialogPanel;

	private Text txt_weapon, txt_weapon_name, txt_weapon_detail;
	private Button btn_pre_gun, btn_next_gun, btn_ok, btn_cancel;
	private InputField ipf_name, ipf_bullet;
	private Toggle tge_bullet;

	private Button m_Ok, m_cancel;
	private Text m_headerText, m_dialogText;

	private Player player;
	private Role role;
	private GameObject[] gunList;
	private int index;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		player = Player.GetInstance ();
		role = new Role ();
		index = 0;
		gunList = GameObject.FindGameObjectsWithTag("gun");
		for(int i = 0; i < gunList.Length; ++i) {
			if (i != index)
				gunList[i].SetActive (false);
		}
		InitUI ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			PreGun ();
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			NextGun ();
		}
	}

	// 切换武器
	private void PreGun(){
		gunList [index].SetActive (false);
		index = (index - 1) % gunList.Length;
		index = index >= 0 ? index : gunList.Length + index;
		gunList [index].SetActive (true);
		txt_weapon.text = txt_weapon_name.text = gunList [index].name;
	}

	private void NextGun(){
		gunList [index].SetActive (false);
		index = (index + 1) % gunList.Length;
		gunList [index].SetActive (true);
		txt_weapon.text = txt_weapon_name.text = gunList [index].name;

	}

	private void InitUI(){
		Text[] texts = canvas.GetComponentsInChildren<Text> ();
		foreach (Text t in texts) {
			switch (t.name) {
			case "weapon_name":
				txt_weapon_name = t;
				break;
			case"weapon_detail":
				txt_weapon_detail = t;
				break;
			case "txt_weapon":
				txt_weapon = t;
				break;
			}
		}
		txt_weapon.text = txt_weapon_name.text = gunList [index].name;
		Button[] buttons = canvas.GetComponentsInChildren<Button> ();
		foreach (Button b in buttons) {
			switch (b.name) {
			case "previous":
				btn_pre_gun = b;
				btn_pre_gun.onClick.AddListener (PreGun);
				break;
			case"next":
				btn_next_gun = b;
				btn_next_gun.onClick.AddListener (NextGun);
				break;
			case "btn_ok":
				btn_ok = b;
				btn_ok.onClick.AddListener (TryCreateRole);
				break;
			case "btn_cancel":
				btn_cancel = b;
				btn_cancel.onClick.AddListener (()=>{SceneManager.LoadScene("select");});
				break;	
			}
		}
		InputField[] inputFields = canvas.GetComponentsInChildren<InputField> ();
		foreach (InputField ipf in inputFields) {
			switch (ipf.name) {
			case "ipf_name":
				ipf_name = ipf;
				break;
			case"ipf_bullet":
				ipf_bullet = ipf;
				break;
			}
		}
		tge_bullet = canvas.GetComponentInChildren<Toggle> ();
		tge_bullet.onValueChanged.AddListener ((value) => {ipf_bullet.gameObject.SetActive (!value);});
		HideDialog ();
	}

	// 处理角色创建请求，超过5秒未返回则超时。
	private void TryCreateRole(){
		if ("".Equals (ipf_name.text) || ("".Equals (ipf_bullet.text) && !tge_bullet.isOn)) {
			ShowDialog ("创建角色", "角色名为空或没有指定弹药量", true, () => {
				HideDialog ();
			}, () => {
				HideDialog ();
			});
			return;
		}
		role.id = 0;
		role.name = ipf_name.text;
		role.level = 1;
		role.maxHP = 100;
		role.exp = 0;
		role.weapon = txt_weapon.text;
		role.attack = 1;
		int bullet;
		if (int.TryParse (ipf_bullet.text, out bullet)) {
			role.ammunition = tge_bullet.isOn ? -1 : bullet;
		} else {
			ShowDialog ("创建角色", "指定弹药量数据不是整数", true, () => {
				HideDialog ();
			}, () => {
				HideDialog ();
			});
			return;
		}
		string data = Processor.C2SCreateRole (player.userId, role);
		Debug.Log (data);
		if (!player.IsLogined()) {
			Debug.LogError ("player do not login!");
			ShowDialog ("创建角色", "与服务器断开或用户未登陆！", true, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			}, () => {
				player.Logout ();
				SceneManager.LoadScene ("start");
			});
		}
		ShowDialog("创建角色", "处理中...", false);
		player.Send (data);
		//异步处理相应，10s仍未获得对应响应则，请求超时
		AsyncMethodCaller caller = new AsyncMethodCaller(Respone);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (10000, true);
		if (!success) {
			Debug.Log ("Time Out");
			ShowDialog ("创建角色", "操作超时", true, () => {
				HideDialog ();
			}, () => {
				HideDialog ();
			});
			gunList [index].SetActive (true);
		} else {
			int returnValue = caller.EndInvoke(result);
			if (returnValue != 0) {
				success = false;
				ShowDialog ("创建角色", "操作失败", true, () => {
					HideDialog ();
				}, () => {
					HideDialog ();
				});
				gunList [index].SetActive (true);
			}
		}
		result.AsyncWaitHandle.Close();
		if (success) {
			Debug.Log ("Success!");
			SceneManager.LoadScene("select");
		}
	}

	private delegate int AsyncMethodCaller();

	private int Respone(){
		while (true) {
			JsonData data = player.Receive();
			if (data != null) {
				string msgname = (string)data ["msgname"];
				if ("Respone".Equals(msgname)) {
					int errcode = (int)data ["errcode"];
					string errmsg = (string)data ["errmsg"];
					if (errcode != 0) {
						Debug.Log (string.Format ("errcode[{0}], errmsg[{0}]", errcode, errmsg));
					} else {
						Debug.Log ("Create role success!");
					}
					return errcode;
				}
			}
		}
	}

	// 显示处理结果的对话框
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
