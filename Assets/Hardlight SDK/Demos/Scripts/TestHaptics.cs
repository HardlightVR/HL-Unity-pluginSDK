/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;

using System.Collections;
using Hardlight.SDK;
using System;

namespace Hardlight.SDK.Demos
{
	public class TestHaptics : MonoBehaviour
	{
		Rigidbody myRB;
	
		/// <summary>
		/// This is controlled based on the suit and contents within NSEnums.
		/// This number exists for easier testing of experimental hardware.
		/// </summary>
		private bool massage = false;
		public float CodeHapticDuration = 5.5f;
		HapticHandle clickerHandle;
		int[] playingIDs;
		//	public Sequence s;

		void Awake()
		{
			
		}
		void Start()
		{
			myRB = GameObject.Find("Haptic Trigger").GetComponent<Rigidbody>();

			//var a = new Sequence("ns.basic.click_click_click");
			//a.CreateHandle(AreaFlag.All_Areas).Play();	

			//clicker.CreateHandle(AreaFlag.All_Areas).Play();
		}


		IEnumerator MoveFromTo(Vector3 pointA, Vector3 pointB, float time)
		{
			while (massage)
			{

				float t = 0f;
				while (t < 1f)
				{
					t += Time.deltaTime / time; // sweeps from 0 to 1 in time seconds
					myRB.transform.position = Vector3.Lerp(pointA, pointB, t); // set position proportional to t
					yield return 0; // leave the routine and return here in the next frame
				}
				t = 0f;

				while (t < 1f)
				{
					t += Time.deltaTime / time; // sweeps from 0 to 1 in time seconds
					myRB.transform.position = Vector3.Lerp(pointB, pointA, t); // set position proportional to t
					yield return 0; // leave the routine and return here in the next frame
				}


			}
		}

		void Update()
		{
			bool moving = false;
			float velVal = 350;

			#region Direction Controls
			if (Input.GetKey(KeyCode.LeftArrow) && myRB.transform.position.x > -8)
			{
				myRB.AddForce(Vector3.left * velVal);
			}
			if (Input.GetKey(KeyCode.RightArrow) && myRB.transform.position.x < 8)
			{
				myRB.AddForce(Vector3.right * velVal);
			}
			if (Input.GetKey(KeyCode.UpArrow) && myRB.transform.position.y < 8)
			{
				myRB.AddForce(Vector3.up * velVal);
			}
			if (Input.GetKey(KeyCode.DownArrow) && myRB.transform.position.y > -8)
			{
				myRB.AddForce(Vector3.down * velVal);
			}

			if (!moving)
			{
				myRB.velocity = Vector3.zero;
			}
			#endregion

			#region Application Quit Code
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}
			#endregion

			if (Input.GetKeyDown(KeyCode.T))
			{
				var s = HapticSequence.CreateNew();
				s.AddEffect(Effect.Click);
				s.Play(AreaFlag.Left_All);
				long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
				Debug.Log("Milliseconds: " + milliseconds);
			}
		}

		public void OnGUI()
		{
			
		

			if (GUI.Button(new Rect(50, 200, 100, 40), "Jolt Left Body"))
			{
				new HapticSequence()
					.AddEffect(Effect.Click)
					.Play(AreaFlag.Left_All);
			}

			if (GUI.Button(new Rect(150, 200, 100, 40), "Jolt Full Body"))
			{
				new HapticSequence()
					.AddEffect(Effect.Click)
					.Play(AreaFlag.All_Areas);
			}

			if (GUI.Button(new Rect(250, 200, 100, 40), "Jolt Right Body"))
			{
				new HapticSequence()
					.AddEffect(Effect.Click)
					.Play(AreaFlag.Right_All);
			}
		}
	}
}
