﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : Health {
	public Slider EXPSlider;
	public Text LevelText;

	public GameObject dialog;
	private Button m_cancel;
	private Text m_headerText, m_dialogText;
	private float dieTime = 0.0f;
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

	void Update () {
		if (dieTime != 0.0f && Time.time > dieTime + 3) {
			BackToSelectScene ();
		}
	}

	new public void Die() {
		ShowDialog ("Endless Shoot", "你已死亡，3秒后返回角色选择界面!");
		dieTime = Time.time;
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

	void BackToSelectScene(){
		SceneManager.LoadScene ("select");
	}
}
