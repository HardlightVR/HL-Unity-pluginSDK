using UnityEngine;
using System.Collections;
using System;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// Handles spacing between two recttransforms.
	/// Used for saying 'Distribute 60% of space to Side A'
	/// </summary>
	public class DividerSlider : MonoBehaviour
	{
		public string MediatorName;

		public enum DividerAxis { Vertical, Horizontal }
		public DividerAxis AxisOfDivision = DividerAxis.Horizontal;

		[Header("First RectTransform")]
		public RectTransform FirstRect;
		public float FirstRectStartingValue;

		[Header("Second RectTransform")]
		public RectTransform SecondRect;
		public float SecondRectStartingValue;

		private Vector2 targetAnchorMax;
		private Vector2 targetAnchorMin;

		private bool dividerObserveMouse;

		[System.Serializable]
		public class AnchorRect
		{
			[SerializeField]
			Vector2 minAnchor = new Vector2(0, 0);
			[SerializeField]
			Vector2 maxAnchor = new Vector2(1, 1);

			public Vector2 MinAnchor
			{
				get
				{
					return minAnchor;
				}

				set
				{
					minAnchor = value;
				}
			}
			public Vector2 MaxAnchor
			{
				get
				{
					return maxAnchor;
				}

				set
				{
					maxAnchor = value;
				}
			}
		}

		void Update()
		{
			//if (Input.GetKey(ActivationKey) && Input.GetMouseButtonDown(0))
			//{
			//	BeginDividerObserve();
			//}

			if (dividerObserveMouse)
			{
				ResolveDivision();
			}
		}

		private void ResolveDivision()
		{
			if (AxisOfDivision == DividerAxis.Horizontal)
			{
				SetHorizontalPercentage(Input.mousePosition.x / Screen.width);
			}
			if (AxisOfDivision == DividerAxis.Vertical)
			{
				SetVerticalPercentage(Input.mousePosition.y / Screen.height);
			}
		}

		public void SetVerticalPercentage(float percentage)
		{
			targetAnchorMin = new Vector2(FirstRect.anchorMin.x, (percentage));
			targetAnchorMax = new Vector2(FirstRect.anchorMax.x, (percentage));

			FirstRect.anchorMax = Vector3.Lerp(FirstRect.anchorMax, targetAnchorMax, 1);
			SecondRect.anchorMin = Vector3.Lerp(SecondRect.anchorMin, targetAnchorMin, 1);
		}

		public void SetHorizontalPercentage(float percentage)
		{
			targetAnchorMin = new Vector2((percentage), FirstRect.anchorMin.y);
			targetAnchorMax = new Vector2((percentage), FirstRect.anchorMax.y);

			FirstRect.anchorMax = Vector3.Lerp(FirstRect.anchorMax, targetAnchorMax, 1);
			SecondRect.anchorMin = Vector3.Lerp(SecondRect.anchorMin, targetAnchorMin, 1);
		}
	}
}