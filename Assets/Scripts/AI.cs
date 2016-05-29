using UnityEngine;
using System.Collections.Generic;
using LitJson;

//怪物的AI控制脚本，当玩家与怪物的的距离小于25单位长度时，向服务器发送路径规划请求，接受到路径信息后，根据位点移动;
//当距离小于10个单位长度时，怪物攻击玩家。
public class AI: MonoBehaviour {
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
	private FootStepSound sound;

	// Use this for initialization
	void Start () {
		path = new List<Vector3> ();
		index = 1;
		slot = 0;
		preAttackTime = 0.0f;
		player = GameObject.FindGameObjectWithTag ("Player");
		animator = this.GetComponentInChildren<Animator> ();
		net = Player.GetInstance ();
		sound = GetComponentInChildren<FootStepSound> ();
	}

	void FixedUpdate (){
        // 计算与玩家的距离
		if (Vector3.Distance (player.transform.position, this.transform.position) < 10.0f) {
			Vector3 target = new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z);
			Vector3 dir = target - this.transform.position;
			if (dir == Vector3.zero) {
				Attack ();
			} else {
				float angle = Vector3.Angle (this.transform.forward, dir);
				if (angle > 10.0f) {
					Vector3 cross = Vector3.Cross (this.transform.forward, dir);
                    // 怪物不是面向玩家，转向玩家，设置相应动画
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
				sound.Play();
			}
		} else if (Vector3.Distance (player.transform.position, this.transform.position) > 25.0f) { //玩家远离了怪物，怪物变为空闲状态
			animator.SetBool ("walk", false);
			animator.SetBool ("right", false);
			animator.SetBool ("left", false);
			path.Clear ();
			index = 1;
			sound.Pause();
		} else {
            // 间隔一段时间向服务器发送怪物及玩家信息，请求获取路径信息。
			if (preSendTime == 0.0f || Time.time > preSendTime + deltaSendTime) {
				string data = Processor.C2SEnemyData (player, id, this.gameObject);
				net.Send (data);
				preSendTime = Time.time;
			}
            //根据路径移动
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
				sound.Play();
			}
		}
	}
    // 设置移动路径
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

    // 攻击玩家
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
