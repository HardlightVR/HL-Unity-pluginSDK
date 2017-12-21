using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK
{
	/// <summary>
	/// Simple data class for defining how the VRObjectMimic should behave
	/// </summary>
	[System.Serializable]
	public class MimickingOptions
	{
		public bool MimicPosition = true;
		public bool MimicRotation = true;
		public bool MimicScale = true;

		public Vector3 ScaleMultiplier = Vector3.one;
		public Vector3 PositionOffset = Vector3.zero;
		public Vector3 EulerOffset = Vector3.zero;

		public MimickingOptions(bool position = true, bool rotation = true, bool scale = true)
		{
			ScaleMultiplier = Vector3.one;
			PositionOffset = Vector3.zero;
			EulerOffset = Vector3.zero;

			MimicPosition = position;
			MimicRotation = rotation;
			MimicScale = scale;
		}
		public MimickingOptions(Vector3 scaleMult, Vector3 positionOffset, Vector3 eulerOffset, bool position = true, bool rotation = true, bool scale = true)
		{
			ScaleMultiplier = scaleMult;
			PositionOffset = positionOffset;
			EulerOffset = eulerOffset;

			MimicPosition = position;
			MimicRotation = rotation;
			MimicScale = scale;
		}
	}
}