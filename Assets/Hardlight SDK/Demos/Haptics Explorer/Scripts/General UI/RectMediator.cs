using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// This class deals with sizing divisions of space between two RectTransforms
	/// This class is not used as the constant rect transform updating creates TERRIBLE framerate.
	/// </summary>
	public class RectMediator : MonoBehaviour
	{
		public string MediatorName;

		[Header("First RectTransform")]
		public RectTransform FirstRect;
		public AnchorRect FirstRectAnchor;
		public AnchorRect TargetFirstRectAnchor;

		[Header("Second RectTransform")]
		public RectTransform SecondRect;
		public AnchorRect SecondRectAnchor;
		public AnchorRect TargetSecondRectAnchor;

		private float timeVal;
		private float mediationTime = .25f;
		private bool mediating;
		private bool positive = true;

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
			//if (Input.GetKeyDown(KeyCode.Delete))
			//{
			//	StartMeditation();
			//}

			if (mediating)
			{
				timeVal += Time.deltaTime * (positive ? 1 : -1);

				if ((positive ? timeVal >= mediationTime : timeVal <= 0))
				{
					timeVal = Mathf.Clamp(timeVal, 0, mediationTime);
					mediating = false;
				}

				FirstRect.anchorMin = Vector2.Lerp(FirstRectAnchor.MinAnchor, TargetFirstRectAnchor.MinAnchor, timeVal / mediationTime);
				FirstRect.anchorMax = Vector2.Lerp(FirstRectAnchor.MaxAnchor, TargetFirstRectAnchor.MaxAnchor, timeVal / mediationTime);

				SecondRect.anchorMin = Vector2.Lerp(SecondRectAnchor.MinAnchor, TargetSecondRectAnchor.MinAnchor, timeVal / mediationTime);
				SecondRect.anchorMax = Vector2.Lerp(SecondRectAnchor.MaxAnchor, TargetSecondRectAnchor.MaxAnchor, timeVal / mediationTime);
			}
		}

		public void StartMeditation()
		{
			mediating = true;
			positive = !positive;
		}
	}
}