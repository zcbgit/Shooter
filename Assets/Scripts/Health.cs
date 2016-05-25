using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Health : MonoBehaviour {
	public Slider HPSlider;
	public GameObject DieEffect;
	public AudioClip[] sounds;

	protected int maxHP = 100;
	protected int HP;
	private AudioSource hitSound;

	private Player net;
	void Start () {
		net = Player.GetInstance ();
		hitSound = GetComponent<AudioSource> ();
	}

	public void Init (int HP){
		this.maxHP = this.HP = HP;
		HPSlider.value = 1.0f;
	}

	void OnTriggerEnter (Collider other) {
		if (HP > 0) {
			int type = -1, victim = -1;
			switch (other.gameObject.tag) {
			case "bullet":
				type = 0;
				break;
			case "missile":
				type = 1;
				break;
			default:
				return;
			}
			int id = -1;
			switch (this.gameObject.tag) {
			case "Player":
				victim = 0;
				break;
			case "Spider":
				victim = 1;
				id = this.gameObject.GetComponent<AI> ().id;
				break;
			case "Mech":
				victim = 2;
				id = this.gameObject.GetComponent<AI> ().id;
				break;	
			} 
			string data = Processor.C2SDamage (type, victim, id);
			net.Send (data);
		}
	}

	public void UpdateHp(int HP){
		if (HP < this.HP) {
			hitSound.clip = sounds [0];
			hitSound.Play ();
		}
		this.HP = HP;
		HPSlider.value = (float)HP / maxHP;
	}

	public void Die() {
		Instantiate (DieEffect, this.transform.position, this.transform.rotation);
		Destroy (this.gameObject);
	}

}
