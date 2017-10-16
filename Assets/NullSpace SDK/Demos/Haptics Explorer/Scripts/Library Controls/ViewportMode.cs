using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK.Demos
{
	public class ViewportMode : MonoBehaviour
	{
		public enum SupportedViewports { TwoDimensional, ThreeDimensional }
		public SupportedViewports MyViewportMode = SupportedViewports.TwoDimensional;
		public KeyCode ActivateHotkey = KeyCode.BackQuote;
		public List<GameObject> ActiveObjects = new List<GameObject>();
		public List<GameObject> ActiveIfDisabledObjects = new List<GameObject>();
		public Camera suitCameraToUse;

		public void Enable(LibraryHapticControls hapticControls)
		{
			SetImportantObjectActivity(true);

			if (suitCameraToUse && hapticControls)
			{
				hapticControls.cam = suitCameraToUse;
				hapticControls.ChangeViewportMode(true);
			}
			else if (suitCameraToUse == null)
			{
				Debug.LogError("[" + name + "] Suit Camera to use is null\n", this);
			}
			else if (hapticControls == null)
			{
				Debug.LogError("[" + name + "] Haptic controls to assign is null\n", this);
			}
		}

		private void SetImportantObjectActivity(bool setToActive)
		{
			for (int i = 0; i < ActiveObjects.Count; i++)
			{
				if (ActiveObjects[i])
				{
					ActiveObjects[i].SetActive(setToActive);
				}
			}
			for (int i = 0; i < ActiveIfDisabledObjects.Count; i++)
			{
				if (ActiveIfDisabledObjects[i])
				{
					ActiveIfDisabledObjects[i].SetActive(!setToActive);
				}
			}
		}

		public void Disable(LibraryHapticControls hapticControls)
		{
			if (hapticControls != null)
			{
				hapticControls.ChangeViewportMode(false);
			}
			else
			{
				Debug.LogError("[" + name + "] Haptic controls to assign is null\n", this);
			}

			SetImportantObjectActivity(false);
		}
	}
}