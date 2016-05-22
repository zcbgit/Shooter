using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : Health {
	public Slider EXPSlider;
	public Text LevelText;

	public GameObject dialog;
	private Button m_Ok, m_cancel;
	private Text m_headerText, m_dialogText;

	private int level;

	public void Init(int HP, int exp, int nextLevelExp, int level) {
		this.maxHP = this.HP = HP;
		HPSlider.value = 1.0f;
		EXPSlider.value = (float)exp / nextLevelExp;
		this.level = level;
		LevelText.text = string.Format ("Level{0}", this.level);
	}

	public void UpdateEXP (int EXP, int nextLevelExp, int level, int maxHP) {
		EXPSlider.value = (float)EXP / nextLevelExp;
		if (level > this.level) {
			this.level = level;
			this.maxHP = maxHP;
			this.HP = (int)(this.maxHP * HPSlider.value);
			LevelText.text = string.Format ("Level{0}", this.level);
		}
	}

	new public void Die() {
		ShowDialog ("Endless Shoot", "你已死亡，返回角色选择界面!", true);
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
