using UnityEngine;
using System.Collections;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// A stub class for future functionality (where the service keeps track of the user dimensions and SDK plugins can query for the dimensions of the current player.
	/// The in-game body then configures based on these dimensions adjusting the character accordingly.
	/// </summary>
	[CreateAssetMenu(menuName = "Hardlight/VR/Body Dimension Data")]
	public class VRBodyDimensions : ScriptableObject
	{
		public bool UpdateEveryFrame;
		/// <summary>
		/// The vertical height of the neck, similar to ForwardAmount (which we use to hang the rest of the non-softbody torso)
		/// </summary>
		public float NeckSize = .1f;
		//How far forward do we put the top of the player's spine (which we use to hang the rest of the non-softbody torso)
		public float ForwardAmount = -.1f;
		//Needed for IMU arms (absolute can figure it out itself)
		public float ShoulderWidth;
		//Necessary for getting the elbow right & for having IMU arms look good
		public float UpperArmLength;

		//Height also matters

		//Depth and width of the torso
		public float UpperTorsoDepth;
		public float LowerTorsoDepth;

		public float UpperTorsoWidth;
		public float LowerTorsoWidth;
	}
}