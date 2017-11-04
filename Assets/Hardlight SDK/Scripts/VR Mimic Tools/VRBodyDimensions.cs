using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Hardlight/VR Body Dimension")]
public class VRBodyDimensions : ScriptableObject 
{
	public bool UpdateEveryFrame; 
	public float NeckSize = .1f;
	public float ForwardAmount = -.1f;
	public float ShoulderWidth;
	public float UpperArmLength;

	public float UpperTorsoDepth;
	public float LowerTorsoDepth;

	public float UpperTorsoWidth;
	public float LowerTorsoWidth;
}