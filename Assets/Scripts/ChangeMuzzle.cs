using UnityEngine;
using System.Collections;

public class ChangeMuzzle : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		this.transform.localScale = Vector3.one * (Random.Range(2,6) / 10.0f);
		this.transform.Rotate(0.0f, 0.0f, Random.Range(0,90));
	}
}
