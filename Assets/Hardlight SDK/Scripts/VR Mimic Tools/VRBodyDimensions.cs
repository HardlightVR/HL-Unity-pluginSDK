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
		[Header("Head Offset")]
		[SerializeField]
		[Range(-2, 2)]
		private float neckSize = .1f;
		[SerializeField]
		[Range(-2, 2)]
		private float forwardAmount = -.4f;

		[Header("Arm Dimensions")]
		[SerializeField]
		[Range(.2f, .65f)]
		private float shoulderWidth = .25f;
		//[Range(.1f, 1.5f)]
		//public float UpperArmLength = .45f;

		[SerializeField]
		[Range(.1f, .8f)]
		private float torsoHeight = .4f;

		[Header("Arm Shoulder Vertical Offset")]
		[SerializeField]
		[Range(.2f, .75f)]
		private float verticalShoulderOffset = .5f;

		[Header("Upper Torso Dimensions")]
		[SerializeField]
		[Range(.1f, .75f)]
		private float upperTorsoWidth = .35f;
		[SerializeField]
		[Range(.1f, .75f)]
		private float upperTorsoHeight = .4f;
		[SerializeField]
		[Range(.05f, 1f)]
		private float upperTorsoDepth = .15f;

		[Header("Lower Torso Dimensions")]
		[SerializeField]
		[Range(.1f, .5f)]
		private float lowerTorsoWidth = .3f;
		[SerializeField]
		[Range(.1f, .5f)]
		private float lowerTorsoHeight = .3f;
		[SerializeField]
		[Range(.05f, 1f)]
		private float lowerTorsoDepth = .1f;

		public Vector3 UpperTorsoDimensions
		{
			get
			{
				return new Vector3(UpperTorsoWidth, UpperTorsoDepth, UpperTorsoHeight);
			}
		}
		public Vector3 LowerTorsoDimensions
		{
			get
			{
				return new Vector3(LowerTorsoWidth, LowerTorsoDepth, LowerTorsoHeight);
			}
		}

		public float NeckSize
		{
			get
			{
				return neckSize;
			}

			set
			{
				neckSize = value;
			}
		}

		public float ForwardAmount
		{
			get
			{
				return forwardAmount;
			}

			set
			{
				forwardAmount = value;
			}
		}

		public float ShoulderWidth
		{
			get
			{
				return shoulderWidth;
			}

			set
			{
				shoulderWidth = value;
			}
		}
		public float TorsoHeight
		{
			get
			{
				return torsoHeight;
			}

			set
			{
				torsoHeight = value;
			}
		}

		public float VerticalShoulderOffset
		{
			get
			{
				return verticalShoulderOffset;
			}

			set
			{
				verticalShoulderOffset = value;
			}
		}

		public float UpperTorsoDepth
		{
			get
			{
				return upperTorsoDepth;
			}

			set
			{
				upperTorsoDepth = value;
			}
		}
		public float UpperTorsoHeight
		{
			get
			{
				return upperTorsoHeight;
			}

			set
			{
				upperTorsoHeight = value;
			}
		}
		public float UpperTorsoWidth
		{
			get
			{
				return upperTorsoWidth;
			}

			set
			{
				upperTorsoWidth = value;
			}
		}

		public float LowerTorsoDepth
		{
			get
			{
				return lowerTorsoDepth;
			}

			set
			{
				lowerTorsoDepth = value;
			}
		}
		public float LowerTorsoHeight
		{
			get
			{
				return lowerTorsoHeight;
			}

			set
			{
				lowerTorsoHeight = value;
			}
		}
		public float LowerTorsoWidth
		{
			get
			{
				return lowerTorsoWidth;
			}

			set
			{
				lowerTorsoWidth = value;
			}
		}
	}
}