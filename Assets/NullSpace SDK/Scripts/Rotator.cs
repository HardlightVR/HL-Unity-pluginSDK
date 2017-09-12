using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public bool rotateX = true;
	public float XRotationSpeed;
	public bool rotateY = true;
	public float YRotationSpeed;
	public bool rotateZ = true;
	public float ZRotationSpeed;

	void Update()
	{
		if (XRotationSpeed != 0 && rotateX)
		{
			transform.Rotate(Vector3.right, XRotationSpeed * Time.deltaTime);
		}
		if (YRotationSpeed != 0 && rotateY)
		{
			transform.Rotate(Vector3.up, YRotationSpeed * Time.deltaTime);
		}
		if (ZRotationSpeed != 0 && rotateZ)
		{
			transform.Rotate(Vector3.forward, ZRotationSpeed * Time.deltaTime);
		}
	}

	public void ToggleRotateX()
	{
		rotateX = !rotateX;
	}
	public void ToggleRotateY()
	{
		rotateY = !rotateY;
	}
	public void ToggleRotateZ()
	{
		rotateZ = !rotateZ;
	}
}
