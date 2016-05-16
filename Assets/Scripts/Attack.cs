using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {
	public GameObject muzzleFlash;
	public GameObject Bullet;
	public GameObject WeaponSlot;
	public int fireRate = 5;

	private bool isFired;
	private int counter;
	private int FireTime;

	private AudioSource fireSound;
	// Use this for initialization
	void Start () {
		fireSound = GetComponent<AudioSource> ();
		muzzleFlash.SetActive (false);
		isFired = false;
		counter = 0;
		FireTime = (int)(1.0f / fireRate / Time.fixedDeltaTime);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			isFired = true;
		} else if (Input.GetMouseButtonUp (0)) {
			isFired = false;
		}
	}

	void FixedUpdate (){
		if (isFired && counter == 0) {
			muzzleFlash.SetActive (true);
			if (!fireSound.isPlaying) {
				fireSound.Play ();
			}
			GameObject bullet = Instantiate (Bullet, WeaponSlot.transform.position, WeaponSlot.transform.rotation) as GameObject;
			counter = (counter + 1) % FireTime;
		} else if (isFired && counter != 0) {
			counter = (counter + 1) % FireTime;
		}else if (!isFired){
			muzzleFlash.SetActive (false);
			fireSound.Pause ();
			counter = 0;
		}
	}

}
