using UnityEngine;
using System.Collections;

public class FacePlayer : MonoBehaviour {
	public GameObject obj;
	private GameObject player;
	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
		obj.transform.LookAt (player.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		obj.transform.LookAt (player.transform.position);
	}
}
