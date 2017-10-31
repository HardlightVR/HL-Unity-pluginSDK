using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAdjustor : MonoBehaviour
{
	public bool rotateX = true;

	[SerializeField]
	private float xRotationSpeed;
	public float XRotationSpeed
	{
		get
		{
			return xRotationSpeed;
		}

		set
		{
			xRotationSpeed = value;
		}
	}

	public bool rotateY = true;

	[SerializeField]
	private float yRotationSpeed;
	public float YRotationSpeed
	{
		get
		{
			return yRotationSpeed;
		}

		set
		{
			yRotationSpeed = value;
		}
	}

	public bool rotateZ = true;

	[SerializeField]
	private float zRotationSpeed;
	public float ZRotationSpeed
	{
		get
		{
			return zRotationSpeed;
		}

		set
		{
			zRotationSpeed = value;
		}
	}

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
			//Quaternion before = transform.rotation;
			//Quaternion after = transform.rotation * (transform.up * .1f)
			xEulerAngle = value;
			var euler = Quaternion.Euler(xEulerAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
			//Debug.Log("Before: " + transform.rotation.eulerAngles + "  " + euler.eulerAngles + "\n", this);
			transform.rotation = euler;
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
