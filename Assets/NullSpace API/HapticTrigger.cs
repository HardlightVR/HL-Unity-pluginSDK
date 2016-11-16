/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
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
        NSManager.Instance.DataModel.Play(new List<HapticEffect> {
            new HapticEffect(Effect.Strong_Click_60, 
            collider.GetComponent<HapticCollider>().regionID, 
            0.0f, 0.0f, 10)
        });
	}
}
