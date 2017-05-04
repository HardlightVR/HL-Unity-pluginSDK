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
			var thing = NSManager.Instance.SamplePlayingStatus();
			for (int i = 0; i < suitObjects.Count; i++)
			{
				float val = (thing[suitObjects[i].regionID]) / 255.0f;
				ColorSuitCollider(suitObjects[i], Color.Lerp(notPlayingColor, playingColor, val));
			}
		}

		public override void OnSuitClicked(SuitBodyCollider clicked, RaycastHit hit)
		{
			Debug.Log("Clicked on " + clicked.name + " with a regionID value of: " + (int)clicked.regionID + "\n");
		}

		public override void OnSuitClicking(SuitBodyCollider clicked, RaycastHit hit)
		{
			Debug.Log("Clicked on " + clicked.name + " with a regionID value of: " + (int)clicked.regionID + "\n");
		}

		public override void OnSuitNoInput()
		{
		}
	}
}