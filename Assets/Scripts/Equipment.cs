using UnityEngine;
using System.Collections;

public class Equipment : MonoBehaviour {
	private Player net; 
	// Use this for initialization
	void Start () {
		net = Player.GetInstance ();
	}
	
	void OnTriggerEnter (Collider other) {
		if ("Player".Equals(other.gameObject.tag)) {
			string data = Processor.C2SGetEquipment ();
			net.Send (data);
			Destroy (this.gameObject);
		}
	}
}
