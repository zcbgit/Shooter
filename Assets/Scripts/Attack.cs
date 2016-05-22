using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Attack : MonoBehaviour {
	public GameObject muzzleFlash;
	public GameObject Bullet;
	public GameObject WeaponSlot;
	public int fireRate = 5;
	public Text TextAmmunition;

	private bool isFired;
	private float deltaTime;
	private float nextTime;
	public int maxAmmunition, ammunition;

	private AudioSource fireSound;
	// Use this for initialization
	void Start () {
		fireSound = GetComponent<AudioSource> ();
		muzzleFlash.SetActive (false);
		deltaTime = 1.0f / fireRate;
		nextTime = 0.0f;
	}

	public void UpdateAmmunition(int value){
		if (value == -1) {
			TextAmmunition.text = string.Format ("弹药量：无限");
		}
		this.ammunition += value;
		TextAmmunition.text = string.Format ("弹药量：{0}", ammunition);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonDown (0)) {
			isFired = true;
		} else if (Input.GetMouseButtonUp (0)) {
			
			isFired = false;
		}
		if (isFired && (ammunition == -1 || ammunition > 0) && (nextTime == 0.0f || Time.time >= nextTime)) {
			if (!fireSound.isPlaying) {
				muzzleFlash.SetActive (true);
				fireSound.Play ();
			}
			ammunition -= 1;
			TextAmmunition.text = string.Format ("弹药量：{0}", this.ammunition);
			Instantiate (Bullet, WeaponSlot.transform.position, WeaponSlot.transform.rotation);
			nextTime = Time.time + deltaTime;
		} else if (!isFired || ammunition == 0) {
			muzzleFlash.SetActive (false);
			fireSound.Pause ();
		}
	}

}
