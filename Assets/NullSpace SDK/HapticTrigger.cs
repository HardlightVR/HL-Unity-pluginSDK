﻿/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/


using UnityEngine;
using System.Collections.Generic;
using NullSpace.SDK;
using NullSpace.SDK.Enums;

/// <summary>
/// Scene-specific script to trigger haptic effects
/// </summary>
public class HapticTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void OnTriggerEnter(Collider collider)
	{
		new Sequence("ns.bump").CreateHandle(collider.GetComponent<HapticCollider>().regionID).Play();
		  
		
	}
}