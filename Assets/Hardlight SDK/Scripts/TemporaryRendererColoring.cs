using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class TemporaryRendererColoring : MonoBehaviour
	{
		private MeshRenderer suitRenderer;
		[SerializeField]
		protected TemporaryColoring nextColoring;
		[SerializeField]
		private Color originalColor;
		[SerializeField]
		protected Color startingColor;

		private enum TemporaryColoringState { ApplyForDuration, ApplyOverDuration, Inactive }
		[SerializeField]
		private TemporaryColoringState ColoringState;

		public bool RevertColorAfterOperation = true;

		[System.Serializable]
		public class TemporaryColoring
		{
			//[SerializeField]
			//float timeValue = .25f;
			public Color color;

			public TemporaryColoring(float time, Color col)
			{
				color = col;
				//timeValue = Mathf.Clamp(time, 0, float.MaxValue);
			}
		}
		public float timeLeft;
		public float lerpRemainingDuration;
		[SerializeField]
		private float lerpDuration;

		public MeshRenderer SuitRenderer
		{
			get
			{
				return suitRenderer;
			}

			set
			{
				suitRenderer = value;
			}
		}

		public Color OriginalColor
		{
			get
			{
				return originalColor;
			}

			set
			{
				originalColor = value;
			}
		}

		public float LerpDuration
		{
			get
			{
				return lerpDuration;
			}

			set
			{
				lerpDuration = value;
			}
		}

		void Update()
		{
			if (ColoringState == TemporaryColoringState.ApplyForDuration)
			{
				timeLeft -= Time.deltaTime;
				SuitRenderer.material.color = nextColoring.color;

				if (timeLeft <= 0)
				{
					nextColoring.color = Color.Lerp(nextColoring.color, OriginalColor, 10 * Time.deltaTime);
					SuitRenderer.material.color = nextColoring.color;
				}
				if (timeLeft <= -.5f)
				{
					ResetToOriginalColor();
				}
			}
			else if (ColoringState == TemporaryColoringState.ApplyOverDuration)
			{
				if (lerpRemainingDuration > 0)
				{
					lerpRemainingDuration -= Time.deltaTime;
				}
				Debug.Log("Lerping [" + startingColor + "] [" + nextColoring.color + "]\n", this);
				var tempColor = Color.Lerp(startingColor, nextColoring.color, 1 - (lerpRemainingDuration / LerpDuration));
				SuitRenderer.material.color = tempColor;
				if (lerpRemainingDuration <= 0)
				{
					if (timeLeft > 0)
					{
						if (RevertColorAfterOperation)
						{
							ColoringState = TemporaryColoringState.ApplyForDuration;
						}
						else
						{
							ColoringState = TemporaryColoringState.Inactive;
						}
					}
					else
					{
						ResetToOriginalColor();
					}
				}
			}
			else
			{

			}
		}

		public void ResetToOriginalColor()
		{
			ColoringState = TemporaryColoringState.Inactive;
			SuitRenderer.material.color = originalColor;
		}

		public void SetOriginalColoring(Color originalColor)
		{
			this.OriginalColor = originalColor;
		}
		public void ApplyTemporaryColoring(Color color, float coloringDuration)
		{
			ColoringState = TemporaryColoringState.ApplyForDuration;
			//Get my current color
			startingColor = SuitRenderer.material.color;

			nextColoring = new TemporaryColoring(coloringDuration, color);
			timeLeft = coloringDuration;
			SuitRenderer.material.color = color;
		}

		public void ApplyTemporaryColoringOverTime(Color targetColor, float lerpDuration, float sustainEndColorDuration = 0)
		{
			if (LerpDuration <= 0)
			{
				ApplyTemporaryColoring(targetColor, sustainEndColorDuration);
			}
			else
			{
				ColoringState = TemporaryColoringState.ApplyOverDuration;
				lerpRemainingDuration = lerpDuration;
				LerpDuration = lerpDuration;
				nextColoring = new TemporaryColoring(sustainEndColorDuration, targetColor);
				timeLeft = lerpDuration;
			}
		}

		//public void ApplyPermanentColoringOverTime(Color targetColor, float lerpDuration)
		//{

		//	ColoringState = TemporaryColoringState.ApplyOverDuration;
		//	lerpRemainingDuration = lerpDuration;
		//	LerpDuration = lerpDuration;
		//	nextColoring = new TemporaryColoring(sustainEndColorDuration, targetColor);
		//	timeLeft = lerpDuration;
		//}

		public static TemporaryRendererColoring CreateTemporaryColoring(MeshRenderer rend, Color originalColor, Color color, float time)
		{
			//Debug.Log("Create temp coloring [" + originalColor + "]\n", rend);
			var coloring = rend.gameObject.GetComponent<TemporaryRendererColoring>();

			if (coloring == null)
			{
				coloring = rend.gameObject.AddComponent<TemporaryRendererColoring>();
			}

			coloring.SuitRenderer = rend;
			coloring.SetOriginalColoring(originalColor);
			coloring.ApplyTemporaryColoring(color, time);

			//Debug.Log("HIT" + rend.name + "  " + color.ToString() + "\n", coloring);
			return coloring;
		}
	}
}