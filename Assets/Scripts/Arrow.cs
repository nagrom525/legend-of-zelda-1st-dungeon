﻿using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

	public GameObject weapon_prefab;
	public GameObject weapon_instance;
	public bool released;
	public GameObject BowInstance;
	public GameObject BowPrefab;
	//public Vector3 pos;

	// Use this for initialization
	void Awake () {
		weapon_instance = weapon_prefab;
		BowInstance = MonoBehaviour.Instantiate (BowPrefab, PlayerControl.instance.transform.position, Quaternion.identity) as GameObject;
		released = false;
	}

	public void Instantiate() {
		weapon_instance = MonoBehaviour.Instantiate(weapon_prefab, PlayerControl.instance.transform.position, Quaternion.identity) as GameObject;
		//BowInstance = MonoBehaviour.Instantiate (BowPrefab, PlayerControl.instance.transform.position, Quaternion.identity) as GameObject;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (released) {
			Vector3 weapon_pos = Vector3.zero;
			if (PlayerControl.instance.current_direction == Direction.NORTH) {
				weapon_pos = new Vector3(0, 1, 0);
			} else if (PlayerControl.instance.current_direction == Direction.EAST) {
				weapon_pos = new Vector3(1, 0, 0);
			} else if (PlayerControl.instance.current_direction == Direction.SOUTH) {
				weapon_pos = new Vector3(0, -1, 0);
			} else if (PlayerControl.instance.current_direction == Direction.WEST) {
				weapon_pos = new Vector3(-1, 0, 0);
			}
			weapon_instance.GetComponent<Rigidbody>().velocity = weapon_pos * 15.0f;
		}
	}

	public void ReleaseArrow() {
			released = true;
	}

	void OnCollisionEnter() {
		//print ("don't get here yet"); 
		if (released) {
			//print("destroy already"); 
			Destroy (weapon_instance);
			Destroy (BowInstance);
		}
	}
}