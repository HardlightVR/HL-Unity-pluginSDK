using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public float XRotationSpeed;
	public float YRotationSpeed;
	public float ZRotationSpeed;

	void Update()
	{
		if (XRotationSpeed != 0)
		{
			transform.Rotate(Vector3.right, XRotationSpeed * Time.deltaTime);
		}
		if (YRotationSpeed != 0)
		{
			transform.Rotate(Vector3.up, YRotationSpeed * Time.deltaTime);
		}
		if (ZRotationSpeed != 0)
		{
			transform.Rotate(Vector3.forward, ZRotationSpeed * Time.deltaTime);
		}
	}
}
