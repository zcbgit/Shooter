using UnityEngine;
using System.Collections;

// 子弹导弹等对象的控制脚本，与其他物体的碰撞通过射线检测。
public class Weapon : MonoBehaviour {
	public float speed = 10.0f;
	public float lifetime = 1.0f;
	public float maxDistance = 100.0f;
	public GameObject destroyEffect = null;

	private float timeout = 0.0f;

	// Use this for initialization
	void Start () {
		timeout = Time.time + lifetime;
	}

	// Update is called once per frame
	void Update () {
		// 物体位置变化较快，通过射线检测。
		RaycastHit hitInfo;
		if (Physics.Raycast (this.transform.position, this.transform.forward, out hitInfo, speed * Time.deltaTime)) {
			if (destroyEffect != null) {
				Instantiate (destroyEffect, this.transform.position, this.transform.rotation);
			}
			Destroy (this.gameObject);
			switch (hitInfo.collider.tag) {
			case "Player":
				hitInfo.collider.gameObject.GetComponent<PlayerHealth> ().Hit (this.tag);
				break;
			case "Mech":
			case "Spider":
				hitInfo.collider.gameObject.GetComponent<Health> ().Hit (this.tag);
				break;
			}
		} else {
			this.transform.position += this.transform.forward * speed * Time.deltaTime;
			maxDistance -= speed * Time.deltaTime;
		}
		if (Time.time > timeout || maxDistance < 0) {
			Destroy (this.gameObject);
		}
	}
}
