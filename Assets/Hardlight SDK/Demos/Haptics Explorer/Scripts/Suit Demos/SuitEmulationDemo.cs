using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	public class SuitEmulationDemo : SuitDemo
	{
		public Color notPlayingColor = new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f);
		public Color playingColor = new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f);

		//Turn on my needed things
		public override void ActivateDemo()
		{
			HandleRequiredObjects(true);

			colorController.SetDefaultColor(notPlayingColor);
			//I need nothing
		}

		//Turn off my needed things
		public override void DeactivateDemo()
		{
			HandleRequiredObjects(false);

			for (int i = 0; i < SuitObjects.Count; i++)
			{
				ColorSuitObject(SuitObjects[i], unselectedColor);
			}
			colorController.SetDefaultColor(unselectedColor);
		}

		public void Update()
		{
			//Our initial emulation techniques 
			var samples = HardlightManager.Instance.SamplePlayingStatus();

			for (int i = 0; i < SuitObjects.Count; i++)
			{
				//Debug.Log(thing.Count);
				if (samples.ContainsKey(SuitObjects[i].regionID))
				{
					float val = (samples[SuitObjects[i].regionID].Strength) / 1000.0f;
					Color color = GetColorByFamily(samples[SuitObjects[i].regionID].Family);
					Color currentColor = GetObjectCurrentColor(SuitObjects[i].gameObject);
					//Debug.Log(samples[SuitObjects[i].regionID].Strength + "\n" + Color.Lerp(color, playingColor, val) + "   " + val, this);
					ColorSuitObject(SuitObjects[i], Color.Lerp(currentColor, Color.Lerp(color, playingColor, val), .85f), .1f, .15f);
				}
			}
		}

		private Color GetColorByFamily(uint family)
		{
			if ((Effect)family == Effect.Click)
			{
				return new Color32(255, 69, 0, 255);
			}
			if ((Effect)family == Effect.Double_Click)
			{
				return new Color32(255, 69, 0, 255);
			}
			if ((Effect)family == Effect.Triple_Click)
			{
				return new Color32(255, 69, 0, 255);
			}
			if ((Effect)family == Effect.Bump)
			{
				return Color.green;
			}
			if ((Effect)family == Effect.Hum)
			{
				return Color.cyan;
			}
			if ((Effect)family == Effect.Tick)
			{
				return Color.blue;
			}
			if ((Effect)family == Effect.Pulse)
			{
				return Color.magenta;
			}
			if ((Effect)family == Effect.Buzz)
			{
				return Color.red;
			}
			if ((Effect)family == Effect.Fuzz)
			{
				return Color.yellow;
			}
			return Color.white;
		}

		public override void OnSuitClicked(HardlightCollider clicked, RaycastHit hit)
		{
			//Debug.Log("Clicked on " + clicked.name + " with a regionID value of: " + (int)clicked.regionID + "\n");
		}

		public override void OnSuitClicking(HardlightCollider clicked, RaycastHit hit)
		{
			//Debug.Log("Clicked on " + clicked.name + " with a regionID value of: " + (int)clicked.regionID + "\n");
		}

		public override void OnSuitNoInput()
		{
		}
	}
}