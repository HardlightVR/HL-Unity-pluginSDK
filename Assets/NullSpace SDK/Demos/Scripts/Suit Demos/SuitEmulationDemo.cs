using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class SuitEmulationDemo : SuitDemo
	{
		public Color unselectedColor = new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f);
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

			for (int i = 0; i < suitObjects.Count; i++)
			{
				ColorSuit(suitObjects[i], unselectedColor);
				ColorSuitCollider(suitObjects[i], unselectedColor);
			}
		}

		public void Update()
		{
			var things = NSManager.Instance.SamplePlayingStatus();
			//foreach (var thing in things)
			//{
			//	Debug.Log(thing.Key + "\n");
			//}
			for (int i = 0; i < suitObjects.Count; i++)
			{
				//Debug.Log(thing.Count);
				if (things.ContainsKey(suitObjects[i].regionID))
				{
					float val = (things[suitObjects[i].regionID].Strength) / 255.0f;
					Color color = GetColorByFamily(things[suitObjects[i].regionID].Family);
					Color currentColor = SuitColliderCurrentColor(suitObjects[i].gameObject);
					ColorSuitCollider(suitObjects[i], Color.Lerp(currentColor, Color.Lerp(color, playingColor, val), .45f));
				}
				else
				{
					ColorSuitCollider(suitObjects[i], notPlayingColor);
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
				return notPlayingColor;
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