using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// This class governs the coloring of MeshRenderers on HardlightCollider objects.
	/// Keeps a dictionary of gameobject keys to TemporaryRenderColoring objects (which handle individual coloring)
	/// </summary>
	public class SuitColorController : MonoBehaviour
	{
		public List<HardlightCollider> suitObjects = new List<HardlightCollider>();
		private Dictionary<GameObject, TemporaryRendererColoring> ColorDict = new Dictionary<GameObject, TemporaryRendererColoring>();
		public SuitRenderers suitRenderers;
		public Color defaultBoxColor;

		void Start()
		{
			suitObjects = FindObjectsOfType<HardlightCollider>().ToList();

			for (int i = 0; i < suitObjects.Count; i++)
			{
				var rend = GetSuitColliderRenderer(suitObjects[i].gameObject);
				CheckDefaultBoxColor(rend);

				ColorDict.Add(suitObjects[i].gameObject, TemporaryRendererColoring.CreateTemporaryColoring(rend, defaultBoxColor, defaultBoxColor, .25f));
			}
		}

		/// <summary>
		/// The source of truth for the default unselected color of the suit.
		/// </summary>
		public Color unselectedColor = new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f);

		#region Color Suit Object (no duration/lerping)
		public void ColorSuitObject(GameObject suitCollider, Color setColor)
		{
			RequestColoring(suitCollider, setColor);
		}
		public void ColorSuitObject(HardlightCollider suitCollider, Color setColor)
		{
			RequestColoring(suitCollider.gameObject, setColor);
		}
		public void ColorSuitObject(int index, Color setColor)
		{
			RequestColoring(suitObjects[index].gameObject, setColor);
		}
		#endregion

		#region Color Suit Object (with duration/lerp)
		/// <summary>
		/// Uses the TemporaryRendererColoring flow rather than the hard-setting flow.
		/// </summary>
		/// <param name="suitCollider">Who needs to be colored (it'll efficiently get & cache the MeshRenderer component)</param>
		/// <param name="setColor">What color should it be. Alpha is discarded (unless you change the material of the renderer)</param>
		/// <param name="sustainDuration">How long to maintain that color before lerping back to the default color (see: SetDefaultColor/unselectedColor)</param>
		/// <param name="lerpDuration">Lerp from current color to the set color? Otherwise provide 0</param>
		public void ColorSuitObject(GameObject suitCollider, Color setColor, float sustainDuration, float lerpDuration)
		{
			RequestColoringOverTime(suitCollider, setColor, sustainDuration, lerpDuration);
		}
		/// <summary>
		/// Uses the TemporaryRendererColoring flow rather than the hard-setting flow.
		/// </summary>
		/// <param name="suitCollider">Who needs to be colored (it'll efficiently get & cache the MeshRenderer component)</param>
		/// <param name="setColor">What color should it be. Alpha is discarded (unless you change the material of the renderer)</param>
		/// <param name="sustainDuration">How long to maintain that color before lerping back to the default color (see: SetDefaultColor/unselectedColor)</param>
		public void ColorSuitObject(HardlightCollider suitCollider, Color setColor, float sustainDuration)
		{
			RequestColoringOverTime(suitCollider.gameObject, setColor, sustainDuration);
		}
		public void ColorSuitObject(int index, Color setColor, float sustainDuration)
		{
			RequestColoringOverTime(suitObjects[index].gameObject, setColor, sustainDuration);
		}

		#endregion

		#region Coloring the entire suit
		/// <summary>
		/// A helper function to color the entire suit without needing to loop through a function.
		/// </summary>
		/// <param name="setColor"></param>
		public void ColorSuit(Color setColor)
		{
			for (int i = 0; i < suitObjects.Count; i++)
			{
				RequestColoring(suitObjects[i].gameObject, setColor);
			}
		}
		/// <summary>
		/// Equivalent to ColorSuit(unselectedColor)
		/// </summary>
		public void UncolorAllSuitColliders()
		{
			ColorSuit(unselectedColor);
		}
		/// <summary>
		/// A function that assigns the TemporaryRendererColoring's inactive color.
		/// If you want the pads by default to be black or orange, call this in ActivateDemo. 
		/// (Make sure to return to the unselectedColor in DeactivateDemo)
		/// </summary>
		/// <param name="newColor"></param>
		public void SetDefaultColor(Color newColor)
		{
			for (int i = 0; i < suitObjects.Count; i++)
			{
				if (ColorDict.ContainsKey(suitObjects[i].gameObject))
				{
					ColorDict[suitObjects[i].gameObject].OriginalColor = newColor;
					RequestColoring(suitObjects[i].gameObject, newColor);
				}
			}
		} 
		#endregion

		#region Core Operations
		/// <summary>
		/// Gets the Mesh Renderer on the suit object and returns the color.
		/// Returns Color.white if an error occurs.
		/// </summary>
		/// <param name="suitCollider">The object (with a MeshRenderer) of questionable color</param>
		/// <returns></returns>
		public Color GetObjectCurrentColor(GameObject suitCollider)
		{
			MeshRenderer rend = GetSuitColliderRenderer(suitCollider);
			return GetObjectCurrentColor(rend);
		}
		public Color GetObjectCurrentColor(MeshRenderer rend)
		{
			if (rend != null)
			{
				return rend.material.color;
			}
			return Color.white;
		}
		private void RequestColoring(GameObject suitObject, Color setColor)
		{
			//if (ColorDict.ContainsKey(suitObject.gameObject))
			//{
			//	ColorDict[suitObject.gameObject].ApplyTemporaryColoringOverTime(duration, setColor, .25f);
			//}
			//else
			//{
			MeshRenderer rend = GetSuitColliderRenderer(suitObject);

			ApplyColorToRenderer(rend, setColor);
			//}
		}
		private void RequestColoringOverTime(GameObject suitObject, Color setColor, float sustainDuration = .25f, float lerpDuration = 0.0f)
		{
			if (ColorDict.ContainsKey(suitObject.gameObject))
			{
				ColorDict[suitObject.gameObject].ApplyTemporaryColoringOverTime(setColor, lerpDuration, sustainDuration);
			}
			else
			{
				MeshRenderer rend = GetSuitColliderRenderer(suitObject);

				CheckDefaultBoxColor(rend);

				sustainDuration = Mathf.Clamp(sustainDuration, .15f, float.MaxValue);
				StartCoroutine(ApplyColorToRendererOverTime(rend, setColor, sustainDuration));
			}
		}

		/// <summary>
		/// Caches the original box color..
		/// See also: SetDefaultColor(Color inactive) - use this to change the default color for a specific mode.
		/// </summary>
		/// <param name="rend"></param>
		private void CheckDefaultBoxColor(MeshRenderer rend)
		{
			if (defaultBoxColor == default(Color) && rend != null)
			{
				defaultBoxColor = rend.material.color;
			}
		}

		/// <summary>
		/// Attempts to use the caching class SuitRenderers.
		/// Will GetComponent every time if SuitRenderers is null.
		/// </summary>
		/// <param name="suitCollider">The GameObject with a mesh renderer</param>
		private MeshRenderer GetSuitColliderRenderer(GameObject suitCollider)
		{
			if (suitRenderers)
			{
				return suitRenderers.GetRenderer(suitCollider);
			}
			return suitCollider.GetComponent<MeshRenderer>();
		}
		/// <summary>
		/// A coroutine based coloring technique. Used if TemporaryRendererColoring technique does not work (due to null refs)
		/// Colors the rendere back to the color it is when this coroutine was started. (Looks ugly if you call many coloring over times at once)
		/// </summary>
		/// <param name="renderer">The renderer to color</param>
		/// <param name="setColor">The color to use</param>
		/// <param name="duration">How long</param>
		private IEnumerator ApplyColorToRendererOverTime(MeshRenderer renderer, Color setColor, float duration)
		{
			var origColor = GetObjectCurrentColor(renderer);
			float stepSize = duration / 10.0f;
			var wait = new WaitForSeconds(stepSize);
			for (float i = 0; i < duration; i += stepSize)
			{
				var colorThisFrame = Color.Lerp(origColor, setColor, i / duration);
				ApplyColorToRenderer(renderer, colorThisFrame);
				yield return wait;
			}
			ApplyColorToRenderer(renderer, setColor);
		}
		/// <summary>
		/// Checks the renderer for null, then applies the color.
		/// (Also checks to ensure we have a default color so we don't destroy that information)
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="setColor"></param>
		private void ApplyColorToRenderer(MeshRenderer renderer, Color setColor)
		{
			if (renderer != null)
			{
				CheckDefaultBoxColor(renderer);
				renderer.material.color = setColor;
			}
		}
		
		#endregion

		#region Coroutines
		public void DisruptAllCoroutines()
		{

		}
		public IEnumerator ApplyDelayedColorToRenderer(GameObject targetObject, Color setColor, float delay, float minDuration = .15f)
		{
			throw new System.NotImplementedException();
			//yield return new WaitForSeconds(Mathf.Clamp(delay, minDuration, float.MaxValue));
			//ApplyColorToRenderer(targetObject, setColor);
		}

		public IEnumerator ColorPadForXDuration(HardlightCollider suit, Color targetColor, Color revertColor, float MinDuration = 0.0f, int steps = 10)
		{
			//I don't think we need to save this local reference. Just in case.
			HardlightCollider current = suit;

			for (int i = 0; i < steps; i++)
			{
				//You could do a fancy color lerp functionality here...
				ColorSuitObject(current, Color.Lerp(targetColor, revertColor, MinDuration / steps));
				yield return new WaitForSeconds(MinDuration / steps);
			}

			//I clamp this to a min of .1 for user visibility.
			//yield return new WaitForSeconds(MinDuration);
			ColorSuitObject(current, revertColor);
		}

		public IEnumerator ColorPadForXDuration(HardlightCollider suit, Color targetColor, Color revertColor, float MinDuration = 0.0f)
		{
			//I don't think we need to save this local reference. Just in case.
			HardlightCollider current = suit;

			//You could do a fancy color lerp functionality here...
			ColorSuitObject(current, targetColor);

			//I clamp this to a min of .1 for user visibility.
			yield return new WaitForSeconds(MinDuration);
			ColorSuitObject(current, revertColor);
		}

		#endregion
	}
}