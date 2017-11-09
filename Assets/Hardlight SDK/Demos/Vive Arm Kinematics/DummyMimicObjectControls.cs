using UnityEngine;
using System.Collections;

public class DummyMimicObjectControls : MonoBehaviour
{
	public bool LeftControls = false;
	public bool RightControls = false;

	public float speed = .01f;
	[SerializeField]
	private bool _listening = false;
	public bool Listening
	{
		get
		{
			return _listening;
		}
		set
		{
			_listening = value;
			Rend.material.color = _listening ? Color.green : Color.white;
		}
	}
	public KeyCode ListenWhen = KeyCode.LeftShift;

	private Vector3 lastMousePos;
	private Vector3 startPos;
	private MeshRenderer rend;
	public MeshRenderer Rend
	{
		get
		{
			if (rend == null)
				Rend = GetComponent<MeshRenderer>();
			return rend;
		}
		set { rend = value; }
	}

	void Start()
	{
		startPos = transform.position;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			transform.position = startPos;
		}

		if (Input.GetKeyDown(ListenWhen))
		{
			Listening = !Listening;
		}

		#region Left
		if ((LeftControls && Listening))
		{
			//Right/Left
			if (Input.GetKey(KeyCode.D))
			{
				transform.position += Vector3.forward * speed;
			}
			if (Input.GetKey(KeyCode.A))
			{
				transform.position -= Vector3.forward * speed;
			}
			//Up/Down
			if (Input.GetKey(KeyCode.Q))
			{
				transform.position += Vector3.right * speed;
			}
			if (Input.GetKey(KeyCode.E))
			{
				transform.position -= Vector3.right * speed;
			}
			//Forward/Backward
			if (Input.GetKey(KeyCode.W))
			{
				transform.position += Vector3.up * speed;
			}
			if (Input.GetKey(KeyCode.S))
			{
				transform.position -= Vector3.up * speed;
			}
		}
		#endregion

		#region Right
		if (RightControls && Listening)
		{
			//Right/Left
			if (Input.GetKey(KeyCode.L))
			{
				transform.position += Vector3.forward * speed;
			}
			if (Input.GetKey(KeyCode.J))
			{
				transform.position -= Vector3.forward * speed;
			}
			//Up/Down
			if (Input.GetKey(KeyCode.U))
			{
				transform.position += Vector3.right * speed;
			}
			if (Input.GetKey(KeyCode.O))
			{
				transform.position -= Vector3.right * speed;
			}
			//Forward/Backward
			if (Input.GetKey(KeyCode.I))
			{
				transform.position += Vector3.up * speed;
			}
			if (Input.GetKey(KeyCode.K))
			{
				transform.position -= Vector3.up * speed;
			}
		}
		#endregion

		#region Mouse
		if (Input.GetMouseButton(0))
		{
			Vector2 wheel = Input.mouseScrollDelta;
			if (wheel.y > 0)
			{
				transform.position += Vector3.right * speed;
			}
			if (wheel.y < 0)
			{
				transform.position -= Vector3.right * speed;
			}

			Vector3 newMousePos = Input.mousePosition;

			//Right/Left
			if (newMousePos.x > lastMousePos.x)
			{
				transform.position += Vector3.forward * speed / 3;
			}
			if (newMousePos.x < lastMousePos.x)
			{
				transform.position -= Vector3.forward * speed / 3;
			}

			//Up/Down
			if (newMousePos.y > lastMousePos.y)
			{
				transform.position += Vector3.up * speed / 3;
			}
			if (newMousePos.y < lastMousePos.y)
			{
				transform.position -= Vector3.up * speed / 3;
			}

			lastMousePos = Input.mousePosition;
		}
		#endregion

	}
}