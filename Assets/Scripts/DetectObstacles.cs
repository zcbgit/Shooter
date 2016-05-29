using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

// 生成场景网格地图
public class DetectObstacles : MonoBehaviour {
	public float beg_x,beg_y, beg_z, end_x, end_y, end_z;
	public string filename;

	private int n_x, n_y, n_z;
	private int x = 0, y = 0, z = 0;
	private int[,,] grid;
	// Use this for initialization
	void Start () {
		n_x = (int)(end_x - beg_x) + 1;
		n_y = (int)(end_y - beg_y) + 1;
		n_z = (int)(end_z - beg_z) + 1;
		grid = new int[n_y,n_z,n_x];
		System.Array.Clear (grid, 0, grid.Length);
		this.transform.position = new Vector3 (beg_x, beg_y, beg_z);
	}
	
	void OnTriggerEnter (Collider other) {
		grid [y,z,x] = 1;
	}

	void FixedUpdate () {
		if (x + 1 == n_x) {
			x = 0;
			if (z + 1 == n_z) {
				z = 0;
				if (y + 1 == n_y) {
					Debug.Log ("地形扫描完成");
					save ();
					Destroy (this.gameObject);
				} else {
					y += 1;
				}
			} else {
				z += 1;
			}
		} else {
			x += 1;
		}
		this.transform.position = new Vector3 (x + beg_x, y + beg_y, z + beg_z);
	}

	void save(){
		FileStream fs = new FileStream (filename, FileMode.OpenOrCreate);
		StreamWriter sw = new StreamWriter (fs);
		for (int i = 0; i < n_y; ++i) {
			for (int j = 0; j < n_z; ++j) {
				for (int k = 0; k < n_x; ++k) {
					sw.Write (grid [i,j,k]);
				}
				sw.Write ("\r\n");
			}
		}
		sw.Flush ();
		sw.Close ();
		fs.Close ();
	}
}
