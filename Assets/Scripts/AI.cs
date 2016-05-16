using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class AI: MonoBehaviour {
	public bool enableAttack;
	public int id;

	private GameObject player;
	private List<Vector3> path;
	private int index;
	private Animator animator;
	//private int walk, forward, back, left, right;
	private Player net;
	private float deltaTime = 0.2f;
	private float latestTime = 0.0f;

	// Use this for initialization
	void Start () {
		enableAttack = false;
		path = new List<Vector3> ();
		index = 0;
		player = GameObject.FindGameObjectWithTag ("Player");
		animator = this.GetComponentInChildren<Animator> ();
		net = Player.GetInstance ();
	}

	void Update () {
		if (path.Count > 0 && index < path.Count && this.transform.position == path [index]) {
			++index;
		}
	}

	void FixedUpdate (){
		if (latestTime == 0.0f || Time.time > latestTime + deltaTime) {
			string data = Processor.C2SEnemyData (net.userId, id, this.gameObject);
			net.Send (data);
			latestTime = Time.time;
		}
		if (path.Count > 0 && index < path.Count && this.transform.position != path [index]) {
			Vector3 dir = path [index] - this.transform.position;
			Vector3 cross = Vector3.Cross (this.transform.forward, dir);
			if ("Spider".Equals (tag)) {
				animator.SetBool ("forward", false);
				animator.SetBool ("back", false);
			} else if ("Mech".Equals (tag)) {
				animator.SetBool ("walk", true);
			}
			animator.SetBool ("left", false);
			animator.SetBool ("right", false);
			float angle = Vector3.Angle (this.transform.forward, dir);
			if (angle > 0) {
				if (cross.y > 0) {
					animator.SetBool ("right", true);
					this.transform.Rotate (this.transform.up, angle > 1.0 ? 1.0f : -angle);
				} else {
					animator.SetBool ("left", true);
					this.transform.Rotate (this.transform.up, angle > 1.0 ? 1.0f : angle);
				}
			} else {
				this.transform.Translate (Vector3.MoveTowards (this.transform.position, path [index], 0.2f));
			}
		} else {
			if ("Spider".Equals (tag)) {
				animator.SetBool ("forward", false);
				animator.SetBool ("back", false);
			} else if ("Mech".Equals (tag)) {
				animator.SetBool ("walk", true);
			}
		}
	}

	public void SetPath(JsonData data){
		path.Clear ();
		index = 0;
		for (int i = 0; i < data.Count; ++i) {
			float x = (float)data[i][0];
			float y = 0.0f;
			float z = (float)data[i][1];
			path.Add (new Vector3 (x, y, z));
		}
	}
}
