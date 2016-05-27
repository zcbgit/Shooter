using UnityEngine;
using System.Collections;

public class FootStepSound : MonoBehaviour {
	
	private AudioSource footstepSound;
	// Use this for initialization
	void Start () {
		footstepSound = GetComponent<AudioSource> ();
	}
	
	public void Play() {
		if (!footstepSound.isPlaying) {
			footstepSound.Play ();
		}
	}

	public void Pause() {
		footstepSound.Pause ();
	}
}
