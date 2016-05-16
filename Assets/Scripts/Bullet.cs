using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
	public float speed = 10.0f;
	public float lifetime = 1.0f;
	public float maxDistance = 100.0f;

	private float timeout = 0.0f;

	// Use this for initialization
	void Start () {
		timeout = Time.time + lifetime;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position += this.transform.forward * speed * Time.deltaTime;
		maxDistance -= speed * Time.deltaTime;
		if (Time.time > timeout || maxDistance < 0) {
			Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter (Collider other) {
		Destroy (this.gameObject);
	}
}
