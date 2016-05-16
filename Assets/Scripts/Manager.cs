using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;

public class Manager : MonoBehaviour {
	public GameObject enemyType1, enemyType2, dialog;
	private Button m_Ok, m_cancel;
	private Text m_headerText, m_dialogText;

	private Player playerNet;
	private GameObject playerObject;
	private Dictionary<int, GameObject> enemies;
	private float deltaTime = 0.2f;
	private float latestTime = 0.0f;

	// Use this for initialization
	void Start () {
		playerNet = Player.GetInstance ();
		playerObject = GameObject.FindGameObjectWithTag ("Player");
		playerObject.GetComponent<Health> ().Init (playerNet.roles [playerNet.selectedRole].blood);
		enemies = new Dictionary<int, GameObject> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (latestTime == 0.0f || Time.time > latestTime + deltaTime) {
			string data = Processor.C2SPlayerData(playerNet.userId, playerNet.roleId, playerObject);
			playerNet.Send (data);
			latestTime = Time.time;
		}
		JsonData jd = playerNet.Receive ();
		if (jd != null) {
			string msgname = (string)jd ["msgname"];
			if (msgname == "CreateEnemy") {
				JsonData data = jd ["data"];
				for (int i = 0; i < data.Count; ++i) {
					int id = (int)data [i] ["id"];
					int type = (int)data [i] ["type"];
					float HP = (float)((double)data [i] ["HP"]);
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
				double HP = (double)jd ["HP"];
				if (id == -1) {
					if (HP > 0.0) {
						playerObject.GetComponent<Health> ().UpdateHp ((float)HP);
					} else {
						ShowDialog ("Endless Shoot", "你已死亡，返回角色选择界面!", true);
					}
				} else {
					if (enemies.ContainsKey (id)) {
						if (HP > 0.0) {
							enemies [id].GetComponent<Health> ().UpdateHp((float)HP);
						} else {
							enemies [id].GetComponent<Health> ().Die ();
						}
					}
				}
			}
		}
	}

	private void ShowDialog(string headerText, string dialogText, bool showButton = false){
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

		if (m_Ok == null || m_cancel == null) {
			Button[] buttons = dialog.GetComponentsInChildren<Button> ();
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
			m_Ok.onClick.AddListener (BackToSelectScene);

			m_cancel.gameObject.SetActive (true);
			m_cancel.onClick.AddListener (BackToSelectScene);
		}
	}

	void BackToSelectScene(){
		SceneManager.LoadScene ("select");
	}
}
