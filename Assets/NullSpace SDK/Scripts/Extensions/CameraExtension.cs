using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public static class CameraExtension
	{
		public static void HideLayer(this Camera camera, int layerToHide)
		{
			if (layerToHide <= 31 && layerToHide >= 0)
			{
				//Define the layermask without the given layer. (usually the Haptics layer)
				camera.cullingMask = camera.cullingMask - (1 << layerToHide);
			}
			else
			{
				if (camera == null)
				{
					Debug.LogError("Invalid Layer [" + layerToHide + "] provided along with a [NULL] camera for hiding" + "\n");
				}
				else
				{
					Debug.LogError("Invalid Layer [" + layerToHide + "] provided to camera [" + camera.name + "]for hiding" + "\n");
				}
			}
		}
	}
}