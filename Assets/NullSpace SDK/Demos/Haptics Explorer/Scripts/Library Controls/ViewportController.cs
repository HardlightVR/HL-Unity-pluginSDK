using UnityEngine;
using System.Collections;
using System;

namespace Hardlight.SDK.Demos
{
	public class ViewportController : MonoBehaviour
	{
		public ViewportMode CurrentViewMode;
		public ViewportMode[] PotentialModes;
		public LibraryHapticControls hapticControls;

		void Start()
		{
			if (hapticControls == null)
			{
				hapticControls = FindObjectOfType<LibraryHapticControls>();
			}

			PotentialModes = FindObjectsOfType<ViewportMode>();
			for (int i = 0; i < PotentialModes.Length; i++)
			{
				PotentialModes[i].Disable(hapticControls);
			}

			if (CurrentViewMode != null)
			{
				SelectViewportMode(CurrentViewMode);
			}
		}

		void Update()
		{
			GetInput();
		}

		private void GetInput()
		{
			for (int i = 0; i < PotentialModes.Length; i++)
			{
				if (PotentialModes[i] != null)
				{
					if (Input.GetKeyDown(PotentialModes[i].ActivateHotkey))
					{
						SelectViewportMode(PotentialModes[i]);
					}
				}
			}
		}

		/// <summary>
		/// Viewport Controller toggles between different predefined viewpoints for the suit (Current: 2D, 3D. Future: Tracked?)
		/// Each ViewportMode handles enabling/disabling of its necessary items (which are set via inspector)
		/// </summary>
		/// <param name="mode"></param>
		public void SelectViewportMode(ViewportMode mode)
		{
			//We don't cause transitions if we're already that demo.
			//if (mode != CurrentViewMode || forceSelect)
			//{
			if (CurrentViewMode != null)
			{
				//Debug.Log("Enabling: " + CurrentDemo.GetType().ToString() + "\t\t" + demo.GetType().ToString() + "\n");
				CurrentViewMode.Disable(hapticControls);
			}
			if (mode != null)
			{
				CurrentViewMode = mode;
				CurrentViewMode.Enable(hapticControls);
			}
			//}
		}

		//Event for 2D/3D switch?
		//Things that care about the 2D/3D switch would subscribe to this and get informed of when they need to update?
	}
}