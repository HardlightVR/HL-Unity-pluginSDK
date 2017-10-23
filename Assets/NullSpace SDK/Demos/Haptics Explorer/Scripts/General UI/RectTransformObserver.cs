using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class RectTransformObserver : MonoBehaviour
{
	public RectTransform Target;
	public Camera myCamera;

	void Awake()
	{
		myCamera = GetComponent<Camera>();
		if (myCamera == null)
		{
			Debug.LogError("RectTransformObserver does not have a camera.\n\t", this);
			gameObject.SetActive(false);
		}
		
		//Debug.Log(myCamera.rect + "\t\t" + Target.rect + "\n\t" + thingB.ToString() + "\n\n", this);
	}

	void Update()
	{
		var thing = RectTransformToScreenSpace(Target);
		Rect thingB = new Rect(
			new Vector2(thing.xMin / Screen.width, thing.yMin / Screen.height),
			new Vector2(thing.xMax / Screen.width - thing.xMin / Screen.width, thing.yMax / Screen.height - thing.yMin / Screen.height));
		myCamera.rect = thingB;
	}

	public static Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - (size * 0.5f), size);
	}
}