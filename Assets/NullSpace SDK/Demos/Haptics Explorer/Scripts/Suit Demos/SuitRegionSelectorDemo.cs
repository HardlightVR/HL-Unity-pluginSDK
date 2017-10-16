/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hardlight.SDK;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// This demo is largely deprecated and will be removed in the future.
	/// </summary>
	public class SuitRegionSelectorDemo : SuitDemo
	{
		private Color selectedColor = new Color(127 / 255f, 227 / 255f, 127 / 255f, 1f);
		bool Adding = false;

		public List<HardlightCollider> SelectedSuitObjects = new List<HardlightCollider>();

		public override void Start()
		{
			SelectedSuitObjects = SuitObjects.ToList();

			base.Start();
		}

		//Turn on my needed things
		public override void ActivateDemo()
		{
			SelectedSuitObjects = SuitObjects.ToList();

			//Set the colors of the suit
			ColorAllSelectedSuitColliders(selectedColor);
		}

		//Turn off my needed things
		public override void DeactivateDemo()
		{
			ColorAllSelectedSuitColliders(unselectedColor);
			SelectedSuitObjects.Clear();
			//But defaulting them to normal colors.
			UncolorAllSuitColliders();
		}

		public override void OnSuitClicked(HardlightCollider clicked, RaycastHit hit)
		{
			Adding = true;

			if (SelectedSuitObjects.Contains(clicked))
			{
				Adding = false;
				SelectedSuitObjects.Remove(clicked);
				ColorSuitObject(clicked, unselectedColor);
			}
			else
			{
				ColorSuitObject(hit.collider.gameObject, selectedColor);
				SelectedSuitObjects.Add(clicked);
			}
		}

		public override void OnSuitClicking(HardlightCollider clicked, RaycastHit hit)
		{
			if (!Adding)
			{
				if (SelectedSuitObjects.Contains(clicked))
				{
					ColorSuitObject(clicked, unselectedColor);
					SelectedSuitObjects.Remove(clicked);
				}
			}
			else
			{
				if (!SelectedSuitObjects.Contains(clicked))
				{
					ColorSuitObject(hit.collider.gameObject, selectedColor);
					SelectedSuitObjects.Add(clicked);
				}
			}
		}

		public override void OnSuitNoInput()
		{
			Adding = false;
		}

		public override void DeselectAllSuitColliders()
		{
			base.DeselectAllSuitColliders();
		}

		public void ColorAllSelectedSuitColliders(Color targetColor)
		{
			for (int i = 0; i < SelectedSuitObjects.Count; i++)
			{
				ColorSuitObject(SelectedSuitObjects[i].gameObject, targetColor);
			}
		}
	}
}