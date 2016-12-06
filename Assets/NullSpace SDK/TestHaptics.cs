/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/


using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using System;
using NullSpace.SDK.Enums;






namespace NullSpace.SDK.Demos
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
		Sequence clicker;
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

			clicker = new Sequence("ns.demos.click_click_click");
			clickerHandle = clicker.CreateHandle(AreaFlag.All_Areas);
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
		}




		public void OnGUI()
		{
			if (GUI.Button(new Rect(500, 100, 100, 50), "ENUMS!"))
			{
				//clicker.CreateHandle(AreaFlag.All_Areas).Play();
				clickerHandle.Play();
			}
			if (GUI.Button(new Rect(600, 100, 100, 50), "ENUMSpause"))
			{
				//clicker.CreateHandle(AreaFlag.All_Areas).Play();
				clickerHandle.Pause();
			}

			if (GUI.Button(new Rect(500, 150, 100,50), "RESUME")) {
				NSManager.Instance.UnfreezeActiveEffects();
			}
			if (GUI.Button(new Rect(600, 150, 100, 50), "PAUSE"))
			{
				NSManager.Instance.FreezeActiveEffects();
			}
			if (GUI.Button(new Rect(700, 150, 100, 50), "CLEAR"))
			{
				NSManager.Instance.ClearAllEffects();
			}
			if (GUI.Button(new Rect(20, 20, 100, 40), "Massage"))
			{
				massage = !massage;
				StartCoroutine(MoveFromTo(new Vector3(0, -3.5f, 0), new Vector3(0, 4.5f, 0), .8f));
			}


			if (GUI.Button(new Rect(160, 80, 150, 40), "Continuous Play All"))
			{
				new Sequence("ns.demos.five_second_hum").CreateHandle(AreaFlag.All_Areas).Play();
			}
			if (GUI.Button(new Rect(20, 140, 100, 40), "Halt All"))
			{
				//model.ClearAllPlaying();
			}

			if (GUI.Button(new Rect(20, 200, 100, 40), "Jolt Left Body"))
			{
				new Sequence("ns.click").CreateHandle(AreaFlag.Left_All).Play();

			}
			if (GUI.Button(new Rect(140, 200, 100, 40), "Jolt Full Body"))
			{
				new Sequence("ns.click").CreateHandle(AreaFlag.All_Areas).Play();

			}
			if (GUI.Button(new Rect(260, 200, 100, 40), "Jolt Right Body"))
			{
				new Sequence("ns.click").CreateHandle(AreaFlag.Right_All).Play();

			}


		}
	}
}
