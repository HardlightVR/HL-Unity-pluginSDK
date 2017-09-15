using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace NullSpace.SDK.Demos
{
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

		public Color unselectedColor = new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f);

		public void ColorSuitObject(GameObject suitCollider, Color setColor)
		{
			RequestColoring(suitCollider, setColor);
		}
		public void ColorSuitObject(HardlightCollider suitCollider, Color setColor)
		{
			RequestColoring(suitCollider.gameObject, setColor);
		}

		public void ColorSuitObject(GameObject suitCollider, Color setColor, float lerpDuration, float sustainDuration)
		{
			RequestColoringOverTime(suitCollider, setColor, lerpDuration, sustainDuration);
		}
		public void ColorSuitObject(int index, Color setColor)
		{
			RequestColoring(suitObjects[index].gameObject, setColor);
		}

		//public void ColorSuitObject(GameObject suitCollider, Color setColor, float lerpDuration, float sustainDuration)
		//{
		//	RequestColoringOverTime(suitCollider, setColor, lerpDuration, sustainDuration);
		//}
		public void ColorSuitObject(HardlightCollider suitCollider, Color setColor, float duration)
		{
			RequestColoringOverTime(suitCollider.gameObject, setColor, duration);
		}
		public void ColorSuitObject(int index, Color setColor, float duration)
		{
			RequestColoringOverTime(suitObjects[index].gameObject, setColor, duration);
		}

		public void UncolorAllSuitColliders()
		{
			ColorSuit(unselectedColor);
		}

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
		private void RequestColoringOverTime(GameObject suitObject, Color setColor, float lerpDuration = 0.0f, float sustainDuration = .25f)
		{
			if (ColorDict.ContainsKey(suitObject.gameObject))
			{
				Debug.Log("NEW stuff\n");
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

		private void CheckDefaultBoxColor(MeshRenderer rend)
		{
			if (defaultBoxColor == default(Color) && rend != null)
			{
				defaultBoxColor = rend.material.color;
			}
		}

		private MeshRenderer GetSuitColliderRenderer(GameObject suitCollider)
		{
			if (suitRenderers)
			{
				return suitRenderers.GetRenderer(suitCollider);
			}
			return suitCollider.GetComponent<MeshRenderer>();
		}
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
		private void ApplyColorToRenderer(MeshRenderer renderer, Color setColor)
		{
			if (renderer != null)
			{
				CheckDefaultBoxColor(renderer);
				renderer.material.color = setColor;
			}
		}
		public void ColorSuit(Color setColor)
		{
			for (int i = 0; i < suitObjects.Count; i++)
			{
				RequestColoring(suitObjects[i].gameObject, setColor);
			}
		}
		#endregion

		#region Coroutines
		public void DisruptAllCoroutines()
		{

		}
		public void ChangeColorDelayed(GameObject targetObject, Color setColor, float delay, float minDuration = .15f)
		{
			throw new System.NotImplementedException();
			var DelayedColoring = StartCoroutine(ApplyDelayedColorToRenderer(targetObject, setColor, delay, minDuration));
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