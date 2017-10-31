using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator2 : MonoBehaviour
{
	public bool rotateX = true;
	public float XRotationSpeed;
	public bool rotateY = true;
	public float YRotationSpeed;
	public bool rotateZ = true;
	public float ZRotationSpeed;

	public bool AllowControls = false;

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
		if (AllowControls)
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				transform.Rotate(Vector3.up, 75 * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				transform.Rotate(Vector3.up, -75 * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				transform.Rotate(Vector3.right, 75 * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				transform.Rotate(Vector3.right, -75 * Time.deltaTime);
			}
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
