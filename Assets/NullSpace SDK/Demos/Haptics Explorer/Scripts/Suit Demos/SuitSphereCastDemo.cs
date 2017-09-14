/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK.Demos
{
	/// <summary>
	/// Largely Empty SuitDemo
	/// Sharing an empty version over the incomplete & buggy one.
	/// </summary>
	public class SuitSphereCastDemo : SuitDemo
	{
		public GameObject ExtentsLimiter;
		public HapticSphereCast spherecast;
		public HapticTrigger greenBox;

		public List<float> playingDurations = new List<float>();
		public List<bool> isPlaying = new List<bool>();
		public List<Rigidbody> myRB = new List<Rigidbody>();
		private int currentRigidbodyIndex = 0;
		public float Extent = 5f;

		private Rigidbody CurrentRigidbody
		{
			get
			{
				return myRB[CurrentRigidbodyIndex];
			}
		}

		public int CurrentRigidbodyIndex
		{
			get
			{
				return currentRigidbodyIndex;
			}

			set
			{
				if (value < myRB.Count)
				{
					CurrentRigidbody.gameObject.SetActive(false);
					currentRigidbodyIndex = value;
					CurrentRigidbody.gameObject.SetActive(true);
				}
			}
		}

		public override void Start()
		{
			//So we can move the green box around
			myRB.Add(spherecast.GetComponent<Rigidbody>());
			myRB.Add(greenBox.GetComponent<Rigidbody>());

			for (int i = 0; i < SuitObjects.Count; i++)
			{
				ColorSuitObject(SuitObjects[i], unselectedColor);

				playingDurations.Add(0);
				isPlaying.Add(false);
			}

			for (int i = 0; i < myRB.Count; i++)
			{
				myRB[i].gameObject.SetActive(false);
			}

			if (CurrentRigidbody)
				CurrentRigidbody.gameObject.SetActive(true);
			base.Start();
		}

		public override void CheckHotkeys()
		{
			#region [Arrows] Direction Controls
			if (CurrentRigidbody != null)
			{
				bool moving = false;
				float velVal = 2500;

				if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
				{
					if (CurrentRigidbody.transform.localPosition.x > -Extent / 2)
					{
						CurrentRigidbody.AddForce(Vector3.left * velVal * Time.deltaTime);
					}
				}
				if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
				{
					if (CurrentRigidbody.transform.localPosition.x < Extent / 2)
					{
						CurrentRigidbody.AddForce(Vector3.right * velVal * Time.deltaTime);
					}
				}
				if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
				{
					if (CurrentRigidbody.transform.localPosition.z < Extent / 2)
					{
						CurrentRigidbody.AddForce(Vector3.forward * velVal * Time.deltaTime);
					}
				}
				if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
				{
					if (CurrentRigidbody.transform.localPosition.z > -Extent / 2)
					{
						CurrentRigidbody.AddForce(Vector3.back * velVal * Time.deltaTime);
					}
				}
				if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.Q))
				{
					if (CurrentRigidbody.transform.localPosition.y < Extent / 2)
					{
						CurrentRigidbody.AddForce(Vector3.up * velVal * Time.deltaTime);
					}
				}
				if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.E))
				{
					if (CurrentRigidbody.transform.localPosition.y > -Extent / 2)
					{
						CurrentRigidbody.AddForce(Vector3.down * velVal * Time.deltaTime);
					}
				}

				if (!moving)
				{
					CurrentRigidbody.velocity = Vector3.zero;
				}
			}
			#endregion

			#region Toggle Massage Hotkey
			if (Input.GetKeyDown(KeyCode.M))
			{
				//ToggleMassage();
			}
			#endregion

			base.CheckHotkeys();
		}

		//Turn on my needed things
		public override void ActivateDemo()
		{
			HandleRequiredObjects(true);
		}

		//Turn off my needed things
		public override void DeactivateDemo()
		{
			HandleRequiredObjects(false);
		}

		public override void OnSuitClicked(HardlightCollider clicked, RaycastHit hit)
		{
			//Click to recalibrate Suit

			//Click to play that pad?
		}

		public override void OnSuitClicking(HardlightCollider suit, RaycastHit hit)
		{ }
		public override void OnSuitNoInput()
		{ }

		public void SetRigidbodyIndex(int rbIndex)
		{
			UncolorAllSuitColliders();
			CurrentRigidbodyIndex = rbIndex;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = lightColor;

			Gizmos.DrawWireCube(ExtentsLimiter.transform.position, Vector3.one * Extent);
		}
	}
}