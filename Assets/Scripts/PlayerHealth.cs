using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : Health {
	public Slider EXPSlider;
	public Text LevelText;
	public AudioClip dieSound;

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
		if (dieSound != null) {
			audioSource.clip = dieSound;
			audioSource.Play ();
		}
	}
		
}
