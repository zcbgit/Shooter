using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using LitJson;

public class CreateSceneHandler : MonoBehaviour {
	public GameObject canvas;
	public GameObject dialogPanel;

	private Text txt_weapon, txt_weapon_name, txt_weapon_detail, txt_dialog_header, txt_dialog_text;
	private Button btn_pre_gun, btn_next_gun, btn_ok, btn_cancel, btn_dialog_ok, btn_dialog_cancel;
	private InputField ipf_name, ipf_bullet;
	private Toggle tge_bullet;

	private Player player;
	private Role role;
	private GameObject[] gunList;
	private int index;

	// Use this for initialization
	void Start () {
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
			case "HeaderText":
				txt_dialog_header = t;
				break;
			case "DialogText":
				txt_dialog_text = t;
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
			case "Ok":
				btn_dialog_ok = b;
				break;
			case "Cancel":
				btn_dialog_cancel = b;
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

	private void TryCreateRole(){
		role.id = 0;
		role.name = ipf_name.text;
		role.level = 1;
		role.maxHP = 100;
		role.exp = 0;
		role.weapon = txt_weapon.text;
		role.attack = 1;
		role.ammunition = tge_bullet.isOn ? -1 : int.Parse (ipf_bullet.text);
		string data = Processor.C2SCreateRole (player.userId, role);
		Debug.Log (data);
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
		ShowDialog("创建角色", "处理中...", false);
		player.Send (data);
		AsyncMethodCaller caller = new AsyncMethodCaller(Respone);
		IAsyncResult result = caller.BeginInvoke(null, null);
		bool success = result.AsyncWaitHandle.WaitOne (5000, true);
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
						Debug.Log ("Login success!");
					}
					return errcode;
				}
			}
		}
	}

	private void ShowDialog(string headerText, string dialogText, bool showButton = false, UnityAction OKListener = null, UnityAction cancelListener = null){
		dialogPanel.SetActive (true);
		txt_dialog_header.text = headerText;
		txt_dialog_text.text = dialogText;

		btn_dialog_ok.gameObject.SetActive (false);
		btn_dialog_cancel.gameObject.SetActive (false);
		if (showButton) {
			btn_dialog_ok.gameObject.SetActive (true);
			if (OKListener != null) {
				btn_dialog_ok.onClick.RemoveAllListeners ();
				btn_dialog_ok.onClick.AddListener (OKListener);
			}
			btn_dialog_cancel.gameObject.SetActive (true);
			if (cancelListener != null) {
				btn_dialog_cancel.onClick.RemoveAllListeners ();
				btn_dialog_cancel.onClick.AddListener (cancelListener);
			}
		}
	}

	private void HideDialog() {
		dialogPanel.SetActive (false);
	}
}
