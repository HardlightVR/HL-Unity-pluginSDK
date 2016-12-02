using UnityEngine;
using System.Collections.Generic;
using NullSpace.SDK;
using NullSpace.API.Enums;
using NullSpace.SDK.Haptics;
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
