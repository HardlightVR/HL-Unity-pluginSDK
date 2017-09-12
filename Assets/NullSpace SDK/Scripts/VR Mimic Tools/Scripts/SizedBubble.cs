using UnityEngine;
using System.Collections;

/// <summary>
/// A sphere representation for collision functions within 
/// </summary>
public class SizedBubble : MonoBehaviour
{
	[SerializeField]
	private float size = .05f;
	public float Size
	{
		get
		{
			return size;
		}

		set
		{
			size = value;
		}
	}
}