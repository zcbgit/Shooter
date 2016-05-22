using UnityEngine;
using System.Collections;

public class DoorControler : MonoBehaviour {
	public bool isOpened;
	private AudioSource doorSound;
	private Animation doorAnimation;
	private Player net;

	// Use this for initialization
	void Start () {
		isOpened = false;
		net = Player.GetInstance ();
		doorSound = GetComponent<AudioSource> ();
		doorAnimation = GetComponent<Animation> ();
	}
	
	void OnTriggerEnter (Collider other) {
		if ("Player".Equals(other.tag)) {
			doorSound.Play ();
			foreach (AnimationState state in doorAnimation) {
				state.time = 0.0f;
				state.speed = 1.0f;
			}
			doorAnimation.Play ();
			isOpened = true;
		} 
	}

	void OnTriggerExit (Collider other){
		if ("Player".Equals(other.tag)) {
			doorSound.Play ();
			foreach (AnimationState state in doorAnimation) {
				state.time = state.length;
				state.speed = -1.0f;
			}
			doorAnimation.Play();
			isOpened = false;
		} 
	}
}
