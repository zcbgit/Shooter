using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class AI: MonoBehaviour {
	public bool enableAttack;
	public int id;
	public GameObject weapon;
	public GameObject[] WeaponSlot;
	public float deltaAttackTime = 0.5f;

	private GameObject player;
	private List<Vector3> path;
	private int index;
	private Animator animator;
	private float preAttackTime;
	private Player net;
	private float deltaSendTime = 2.0f;
	private float preSendTime = 0.0f;
	private int slot;

	private LineRenderer liner1;

	// Use this for initialization
	void Start () {
		enableAttack = false;
		path = new List<Vector3> ();
		index = 1;
		slot = 0;
		preAttackTime = 0.0f;
		player = GameObject.FindGameObjectWithTag ("Player");
		animator = this.GetComponentInChildren<Animator> ();
		net = Player.GetInstance ();

//		liner1 = this.gameObject.AddComponent<LineRenderer> () as LineRenderer;
//		liner1.SetWidth (0.1f, 0.1f);
//		liner1.SetColors(Color.red, Color.yellow);
//		liner1.SetVertexCount (4);
	}

	void FixedUpdate (){
		if (Vector3.Distance (player.transform.position, this.transform.position) < 10.0f) {
			Vector3 target = new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z);
			Vector3 dir = target - this.transform.position;
			if (dir == Vector3.zero) {
				Attack ();
			} else {
				float angle = Vector3.Angle (this.transform.forward, dir);
				if (angle > 10.0f) {
					Vector3 cross = Vector3.Cross (this.transform.forward, dir);
					if (cross.y > 0) {
						if (this.tag.Equals ("Mech")) {
							animator.SetBool ("walk", false);
							animator.SetBool ("left", false);
							animator.SetBool ("right", true);
						} else if (this.tag.Equals ("Spider")) {
							animator.SetBool ("walk", true);
						}
						this.transform.Rotate (Vector3.up, angle > 1.0 ? 1.0f : angle);
					} else {
						if (this.tag.Equals ("Mech")) {
							animator.SetBool ("walk", false);
							animator.SetBool ("right", false);
							animator.SetBool ("left", true);
						} else if (this.tag.Equals ("Spider")) {
							animator.SetBool ("walk", true);
						}
						this.transform.Rotate (Vector3.up, angle > 1.0 ? -1.0f : -angle);
					}
				} else {
					animator.SetBool ("walk", false);
					animator.SetBool ("right", false);
					animator.SetBool ("left", false);
					Attack ();
				}
			}
		} else if (Vector3.Distance (player.transform.position, this.transform.position) > 25.0f) {
			animator.SetBool ("walk", false);
			animator.SetBool ("right", false);
			animator.SetBool ("left", false);
			path.Clear ();
			index = 1;
		} else {
			if (preSendTime == 0.0f || Time.time > preSendTime + deltaSendTime) {
				string data = Processor.C2SEnemyData (player, id, this.gameObject);
				net.Send (data);
				preSendTime = Time.time;
			}
			if (path.Count > 0 && index < path.Count) {
				Vector3 target = path [index];
				Vector3 dir = target - this.transform.position;
				if (Vector3.Distance(Vector3.zero, dir) < 0.1f) {
					++index;
					return;
				}
				float angle = Vector3.Angle (this.transform.forward, dir);
				if (angle > 1.0f) {
					Vector3 cross = Vector3.Cross (this.transform.forward, dir);
					animator.SetBool ("walk", false);
					if (cross.y > 0) {
						animator.SetBool ("left", false);
						animator.SetBool ("right", true);
						this.transform.Rotate (Vector3.up, angle > 1.0 ? 1.0f : angle);
					} else {
						animator.SetBool ("right", false);
						animator.SetBool ("left", true);
						this.transform.Rotate (Vector3.up, angle > 1.0 ? -1.0f : -angle);
					}
				} else {
					animator.SetBool ("right", false);
					animator.SetBool ("left", false);
					animator.SetBool ("walk", true);
					this.transform.position = Vector3.MoveTowards (this.gameObject.transform.position, target, 0.025f);
				}
			}
		}
	}

	public void SetPath(JsonData data){
		path.Clear ();
		for (int i = 0; i < data.Count; ++i) {
			float x = (float)((double)data [i][0]);
			float y = this.transform.position.y;
			float z = (float)((double)data [i][1]);
			path.Add (new Vector3 (x, y, z));
		}
		index = 1;
	}

	void Attack (){
		if (preAttackTime == 0.0f || Time.time > preAttackTime + deltaAttackTime) {
			GameObject go = Instantiate (weapon, WeaponSlot [slot].transform.position, WeaponSlot [slot].transform.rotation) as GameObject;
			Vector3 target = player.transform.position;
			target.y = 0.6f;
			go.transform.LookAt (target);
			WeaponSlot [slot].GetComponent<AudioSource> ().Play ();
			preAttackTime = Time.time;
			slot = (slot + 1) % WeaponSlot.Length;
		}
	}
}
