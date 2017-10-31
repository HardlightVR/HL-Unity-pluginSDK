using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// This demo is largely outdated. It primarily served for the 2D version. See AdvancedMassageSuitDemo
	/// </summary>
	public class SuitMassageDemo : SuitDemo
	{
		public HapticTrigger greenBox;
		public Color selectedColor = new Color(100 / 255f, 200 / 255f, 200 / 255f, 1f);
		public List<float> playingDurations = new List<float>();
		public List<bool> isPlaying = new List<bool>();
		private bool massage = false;
		Rigidbody myRB;

		/// <summary>
		/// Boundary confines for the green box.
		/// </summary>
		public float Extent = 5f;

		/// <summary>
		/// Note: the reason we care about duration is this version of the haptics API doesn't let us see what is currently playing/not playing.
		/// Therefore we can't visuall represent what is actually happening on the suit.
		/// </summary>
		private float duration = .5f;
		private float lerpColorOut = .25f;
		private float minDuration = .05f;

		public float Duration
		{
			get
			{
				return duration;
			}

			set
			{
				duration = value;
			}
		}

		public override void Start()
		{
			//So we can move the green box around
			myRB = greenBox.GetComponent<Rigidbody>();

			for (int i = 0; i < SuitObjects.Count; i++)
			{
				ColorSuitObject(SuitObjects[i], selectedColor);

				playingDurations.Add(0);
				isPlaying.Add(false);
			}
			base.Start();
		}

		public void Update()
		{
			for (int i = 0; i < playingDurations.Count; i++)
			{
				if (isPlaying[i])
				{
					playingDurations[i] = Mathf.Clamp(playingDurations[i] - Time.deltaTime, 0, 1000);
					if (playingDurations[i] <= 0)
					{
						ColorSuitObject(SuitObjects[i], unselectedColor);
						isPlaying[i] = false;
					}
					else if (playingDurations[i] <= lerpColorOut)
					{
						//This handles decoloring locations that have finished their duration (and can be reset)
						ColorSuitObject(SuitObjects[i], Color.Lerp(unselectedColor, selectedColor, playingDurations[i] / lerpColorOut));
					}
				}
			}
		}

		public override void CheckHotkeys()
		{
			#region [Arrows] Direction Controls
			bool moving = false;
			float velVal = 350;

			if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && myRB.transform.position.x > -Extent)
			{
				myRB.AddForce(Vector3.left * velVal);
			}
			if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && myRB.transform.position.x < Extent)
			{
				myRB.AddForce(Vector3.right * velVal);
			}
			if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) && myRB.transform.position.y < Extent)
			{
				myRB.AddForce(Vector3.up * velVal);
			}
			if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && myRB.transform.position.y > -Extent)
			{
				myRB.AddForce(Vector3.down * velVal);
			}

			if (!moving)
			{
				myRB.velocity = Vector3.zero;
			}
			#endregion

			#region Toggle Massage Hotkey
			if (Input.GetKeyDown(KeyCode.M))
			{
				ToggleMassage();
			} 
			#endregion

			base.CheckHotkeys();
		}

		public override void ActivateDemo()
		{
			HandleRequiredObjects(true);
		}

		//Turn off my needed things
		public override void DeactivateDemo()
		{
			HandleRequiredObjects(false);
			ClearAll();
		}

		public override void OnSuitClicked(HardlightCollider clicked, RaycastHit hit)
		{
		}

		public override void OnSuitClicking(HardlightCollider clicked, RaycastHit hit)
		{
		}

		public override void OnSuitNoInput()
		{
		}

		public override void DisplayHandleHaptic(HardlightCollider hit, HapticHandle handle)
		{
			//This could be done more efficiently. It is kept simple to make the code more readible.
			int index = SuitObjects.IndexOf(hit);

			//If the current duration is over.
			if (playingDurations[index] <= 0)
			{
				isPlaying[index] = true;
				//Note to self: Use the handle remaining duration.

				//We enforce a minimum duration mostly to ensure there is a good visual.
				playingDurations[index] = Mathf.Clamp(Duration, minDuration, float.MaxValue);

				//Color the suit (drawn haptic expiration handles decoloring.
				ColorSuitObject(hit, selectedColor);
			}
			else
			{
				//Don't do anything
			}
		}

		private void ClearAll()
		{
			for (int i = 0; i < isPlaying.Count; i++)
			{
				ClearIndex(i);
			}
		}
		private void ClearIndex(int index)
		{
			if (isPlaying.Count > index)
			{
				isPlaying[index] = false;
			}
			if (playingDurations.Count > index)
			{
				playingDurations[index] = 0.0f;
			}

			ColorSuitObject(index, unselectedColor);
		}

		public void ToggleMassage()
		{
			//For moving the green box to auto-play the last played sequence.
			//You can probably do something more inspired for an 'actual massage'
			massage = !massage;
			StartCoroutine(MoveFromTo(new Vector3(0, -3.5f, 0), new Vector3(0, 5.8f, 0), .8f));
		}
		//Move the massaging green box up and down.
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

	}
}