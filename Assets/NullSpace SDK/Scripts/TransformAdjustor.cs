using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAdjustor : MonoBehaviour
{
	public bool rotateX = true;
	public float XRotationSpeed;
	public bool rotateY = true;
	public float YRotationSpeed;
	public bool rotateZ = true;
	public float ZRotationSpeed;

	#region Euler Properties
	private float xEulerAngle;
	public float XEulerAngle
	{
		get
		{
			return transform.eulerAngles.x;
		}

		set
		{
			xEulerAngle = value;
			transform.rotation = Quaternion.Euler(xEulerAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
		}
	}

	private float yEulerAngle;
	public float YEulerAngle
	{
		get
		{
			return transform.eulerAngles.y;
		}

		set
		{
			yEulerAngle = value;
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, yEulerAngle, transform.rotation.eulerAngles.z);
		}
	}

	private float zEulerAngle;
	public float ZEulerAngle
	{
		get
		{
			return transform.eulerAngles.y;
		}

		set
		{
			zEulerAngle = value;
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, zEulerAngle);
		}
	} 
	#endregion

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
