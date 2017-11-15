using UnityEngine;
using System.Collections;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	[CreateAssetMenu(menuName = "Hardlight/VR/Body Dimension Data")]
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
}