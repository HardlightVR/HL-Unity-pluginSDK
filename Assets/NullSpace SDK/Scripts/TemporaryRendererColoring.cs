using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class TemporaryRendererColoring : MonoBehaviour
	{
		protected MeshRenderer rend;
		protected TemporaryColoring nextColoring;
		protected Color originalColor;
		public class TemporaryColoring
		{
			float timeValue = .25f;
			public Color color;

			public TemporaryColoring(float time, Color col)
			{
				color = col;
				timeValue = time;
			}
		}
		public float timeLeft;

		void Start()
		{

		}

		void Update()
		{
			if (timeLeft > 0)
			{
				timeLeft -= Time.deltaTime;
				
			}
			if (timeLeft <= 0)
			{
				nextColoring.color = Color.Lerp(nextColoring.color, originalColor, 10f * Time.deltaTime);
				rend.material.color = nextColoring.color;
			}
		}

		public void AddColoring(TemporaryColoring tempColoring)
		{
			//Add it to our list.
		}

		public static TemporaryRendererColoring ApplyTemporaryColoring(MeshRenderer rend, float time, Color color, Color originalColor)
		{
			//Debug.Log("apply temp color\n", rend);
			var coloring = rend.gameObject.GetComponent<TemporaryRendererColoring>();

			if (coloring == null)
			{
				coloring = rend.gameObject.AddComponent<TemporaryRendererColoring>();
			}

			coloring.originalColor = originalColor;
			coloring.rend = rend;
			coloring.nextColoring = new TemporaryColoring(time, color);
			coloring.timeLeft = time;
			coloring.rend.material.color = color;

			//Debug.Log("HIT" + rend.name + "  " + color.ToString() + "\n", coloring);
			return coloring;
		}
	}
}