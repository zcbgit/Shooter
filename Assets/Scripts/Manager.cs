using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading;
using LitJson;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {
	public GameObject enemyType1, enemyType2;
	public GameObject[] Guns;
	public GameObject equipment;
	public Text killedText;
	public GameObject dialog;
	public GameObject menu;

	private Text m_headerText, m_dialogText;
	private Player playerNet;
	private GameObject playerObject;
	private Role role;
	private PlayerHealth ph;
	private Attack ak;
	private Dictionary<int, GameObject> enemies;
	private float deltaTime = 10.0f;
	private float latestTime = 0.0f;
	private int killedCount = 0;
	private float dieTime = 0.0f;
	private bool isPause = false;

	// Use this for initialization
	void Start () {
		InitMenu ();
		playerNet = Player.GetInstance ();
		playerObject = GameObject.FindGameObjectWithTag ("Player");
		role = playerNet.roles [playerNet.selectedRole];
		ph = playerObject.GetComponent<PlayerHealth> ();
		ph.Init (role.maxHP, role.exp, role.nextLevelExp, role.level);
		GameObject gun = null;
		foreach (GameObject go in Guns) {
			if (role.weapon.Equals (go.name)) {
				go.SetActive(true);
				gun = go;
				break;
			}
		}
		ak = gun.GetComponent<Attack> ();
		ak.UpdateAmmunition (role.ammunition);
		enemies = new Dictionary<int, GameObject> ();
		killedText.text = string.Format ("击杀数：{0}", killedCount);

	}

	void InitMenu() {
		menu.SetActive (false);
		Button[] buttons = menu.GetComponentsInChildren<Button> ();
		foreach (Button b in buttons){
			switch (b.name){
			case "resume":
				b.onClick.AddListener (() => {
					menu.SetActive (false);
					isPause = false;
					Time.timeScale = 1.0f;
				});
				break;
			case "select":
				b.onClick.AddListener (() => {
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					SceneManager.LoadScene ("select");
				});
				break;	
			case "logout":
				b.onClick.AddListener (() => {
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					playerNet.Logout();
					SceneManager.LoadScene ("start");
				});
				break;
			case "quite":
				b.onClick.AddListener (() => {
					Application.Quit();
				});
				break;
			}				
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (!isPause) {
				
				Time.timeScale = 0.0f;
				isPause = true;
				menu.SetActive (true);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			} else {
				menu.SetActive (false);
				Time.timeScale = 1.0f;
				isPause = false;
			}
		}

		if (dieTime != 0.0f && Time.time > dieTime + 3) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			SceneManager.LoadScene ("select");
		}

		if (latestTime == 0.0f || Time.time > latestTime + deltaTime) {
			string data = Processor.C2SPlayerData(playerNet.userId, playerNet.roleId, playerObject);
			playerNet.Send (data);
			latestTime = Time.time;
		}
		JsonData jd = playerNet.Receive ();
		while (jd != null) {
			ProcessJD (jd);
			jd = playerNet.Receive ();
		}
	}

	void ProcessJD(JsonData jd){
		if (jd != null) {
			string msgname = (string)jd ["msgname"];
			if (msgname == "CreateEnemy") {
				JsonData data = jd ["data"];
				for (int i = 0; i < data.Count; ++i) {
					int id = (int)data [i] ["id"];
					int type = (int)data [i] ["type"];
					int HP = (int)data [i] ["HP"];
					float x = (float)((double)data [i] ["x"]);
					float z = (float)((double)data [i] ["z"]);
					switch (type) {
					case 0:
						GameObject enemy0 = Instantiate (enemyType1, new Vector3 (x, 0.0f, z), this.transform.rotation) as GameObject;
						enemy0.GetComponent<AI> ().id = id;
						enemy0.GetComponent<Health> ().Init (HP);
						enemies [id] = enemy0;
						break;
					case 1:
						GameObject enemy1 = Instantiate (enemyType2, new Vector3 (x, 0.0f, z), this.transform.rotation) as GameObject;
						enemy1.GetComponent<AI> ().id = id;
						enemies [id] = enemy1;
						enemy1.GetComponent<Health> ().Init (HP);
						break;
					}
				}

			} else if (msgname == "UpdateHP") {
				int id = (int)jd ["id"];
				int HP = (int)jd ["HP"];
				if (id == -1) {
					if (HP > 0) {
						ph.UpdateHp (HP);
					} else {
						ph.UpdateHp (0);
						dieTime = Time.time;
						ShowDialog ("Endless Shoot", "你已死亡，将在3秒后返回！");
						ph.Die ();
					}
				} else {
					if (enemies.ContainsKey (id)) {
						if (HP > 0) {
							enemies [id].GetComponent<Health> ().UpdateHp (HP);
						} else {
							enemies [id].GetComponent<Health> ().Die ();
							killedCount += 1;
							killedText.text = string.Format ("击杀数：{0}", killedCount);
						}
					}
				}
			} else if (msgname == "UpdateAmmunition") {
				int ammunition = (int)jd ["ammunition"];
				ak.UpdateAmmunition (ammunition);
			} else if (msgname == "LevelUp") {
				int exp = (int)jd ["EXP"];
				int nextLevelExp = (int)jd ["NextLevelExp"];
				int level = (int)jd ["Level"];
				int HP = (int)jd ["HP"];
				ph.UpdateEXP (exp, nextLevelExp, level, HP);
			} else if (msgname == "CreateEquipment") {
				float x = (float)((double)jd ["X"]);
				float z = (float)((double)jd ["Z"]);
				Instantiate (equipment, new Vector3 (x, 1.0f, z), this.transform.rotation);
			} else if (msgname == "UpdatePath") {
				int id = (int)jd ["id"];
				if (enemies.ContainsKey (id)) {
					JsonData path = jd["path"];
					enemies [id].GetComponent<AI> ().SetPath (path);
				}
			}
		}
	}

	private void ShowDialog(string headerText, string dialogText){
		dialog.SetActive (true);
		if (m_headerText == null || m_dialogText == null) {
			Text[] texts = dialog.GetComponentsInChildren<Text> ();
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
	}
}
