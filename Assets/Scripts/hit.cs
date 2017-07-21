﻿using UnityEngine;
using System.Collections;

public class hit : MonoBehaviour {
	public int checkpoints;
	public Material Passed;
	public Vector3 init_pos;
	public Quaternion init_rotation;
	public bool crash;

	// Use this for initialization
	void Start () {
		crash = false;
		checkpoints = 0;
		init_pos = transform.position;
		init_rotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Checkpoint") {
			Renderer tmp = other.gameObject.GetComponent<Renderer> ();
			Checkpoint t = other.gameObject.GetComponent<Checkpoint>();
            if (!t)
            {
                return;
            }
            bool p = t.passed;
			if(!p){
				t.SetBool(true);
				tmp.material = Passed;
				checkpoints++;
			}
		} else {
			//hit a wall considered fail
			//checkpoints = 0;
			crash = true;
		}
	}
}
