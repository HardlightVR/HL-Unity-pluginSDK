using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class SuitEmulationDemo : SuitDemo
	{
		public Color notPlayingColor = new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f);
		public Color playingColor = new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f);

		//Turn on my needed things
		public override void ActivateDemo()
		{
			HandleRequiredObjects(true);

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
		}

		public void Update()
		{
			var things = NSManager.Instance.SamplePlayingStatus();


			
			for (int i = 0; i < SuitObjects.Count; i++)
			{
				if (things.ContainsKey(SuitObjects[i].displayID))
				{

					var theThing = things[SuitObjects[i].displayID];

					if (theThing.Strength == 0)
					{
						ColorSuitObject(SuitObjects[i], notPlayingColor);
						continue;
					}
					float val = (theThing.Strength) / 255.0f;
					Color color = GetColorByFamily(theThing.Family);
					Color currentColor = GetObjectCurrentColor(SuitObjects[i].gameObject);
					ColorSuitObject(SuitObjects[i], new Color(1, 1, 1, val));
					//ColorSuitObject(SuitObjects[i], Color.Lerp(currentColor, Color.Lerp(color, playingColor, val), .45f));
				}
				else
				{
					ColorSuitObject(SuitObjects[i], notPlayingColor);
				}
			}
		}

		private Color GetColorByFamily(uint family)
		{
			if ((Effect)family == Effect.Click)
			{
				return Color.red;
			}
			if ((Effect)family == Effect.Double_Click)
			{
				return Color.red;
			}
			if ((Effect)family == Effect.Triple_Click)
			{
				return Color.red;
			}
			if ((Effect)family == Effect.Bump)
			{
				return Color.green;
			}
			if ((Effect)family == Effect.Buzz)
			{
				return Color.cyan;
			}
			if ((Effect)family == Effect.Tick)
			{
				return Color.blue;
			}
			if ((Effect)family == Effect.Hum)
			{
				return Color.yellow;
			}
			if ((Effect)family == Effect.Pulse)
			{
				return new Color32(255, 69, 0, 255);
			}
			if ((Effect)family == Effect.Fuzz)
			{
				return Color.magenta;
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