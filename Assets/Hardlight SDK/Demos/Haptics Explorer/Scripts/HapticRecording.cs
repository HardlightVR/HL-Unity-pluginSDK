using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// This is a test approach to the concept of recording haptics at certain playtimes then saving them or replaying them later.
	/// </summary>
	public class HapticRecording : MonoBehaviour
	{
		//public CanvasGroup QueryForNewFileName;
		//public Button RecordingButton;
		[SerializeField]
		private bool isRecording = false;
		float timeSinceRecordingStart = 0;

		[System.Serializable]
		public class TimedSerializableHaptic
		{
			public ScriptableObjectHaptic haptic;
			private float playTime;
			public float PlayTime
			{
				get
				{
					return playTime;
				}
				set
				{
					playTime = Mathf.Clamp(value, 0, float.MaxValue);
				}
			}

			public TimedSerializableHaptic(ScriptableObjectHaptic newHaptic, float time = 0.0f)
			{
				haptic = newHaptic;
				PlayTime = time;
			}

			public override string ToString()
			{
				string output = "{\"time\" : " + PlayTime + ", \"" + haptic.GetType().ToString() + "\" : \"thing\"}";
				return output;
			}
		}

		[SerializeField]
		public List<TimedSerializableHaptic> recordedHaptics = new List<TimedSerializableHaptic>();

		public bool IsRecording
		{
			get
			{
				return isRecording;
			}

			set
			{
				isRecording = value;
			}
		}

		public void StartRecording()
		{
			Debug.Log("Start Recording\n");
			IsRecording = true;
			timeSinceRecordingStart = 0.0f;
		}

		public void EndRecording()
		{
			IsRecording = false;
			OutputRecordedHapticInfo();
		}

		public void FinishRecording()
		{
			Debug.Log("End Recording\n");
			OutputRecordedHapticInfo();
			EndRecording();

			//If auto-naming files
			//	

			//Else

			//Open the save as file information
			//Add the delegate for naming the file?

			ClearRecordedHaptics();
		}

		public void ClearRecordedHaptics()
		{
			OutputRecordedHapticInfo();

			IsRecording = false;
			timeSinceRecordingStart = 0.0f;
			recordedHaptics.Clear();
		}
		public void PlayRecordedHaptics()
		{
			OutputRecordedHapticInfo();
			StartCoroutine(Playback(recordedHaptics.ToList()));
		}

		public void Update()
		{
			if (IsRecording)
			{
				timeSinceRecordingStart += Time.deltaTime;
			}
			
			GetInput();
		}

		void GetInput()
		{
			if (Input.GetKeyDown(KeyCode.ScrollLock))
			{
				OutputRecordedHapticInfo();
			}
		}

		private void OutputRecordedHapticInfo()
		{
			System.Text.StringBuilder ss = new System.Text.StringBuilder("Recorded Haptics:\n");
			for (int i = 0; i < recordedHaptics.Count; i++)
			{
				ss.AppendLine(recordedHaptics[i].ToString());
			}
			Debug.Log(ss.ToString() + "\n");

		}

		public void AddNewHaptic(SerializableHaptic newHaptic)
		{
			Debug.Log("Add new haptic disabled\n");
			//recordedHaptics.Add(new TimedSerializableHaptic(newHaptic, timeSinceRecordingStart));
		}

		public void AddNewHaptic(float specificTime, SerializableHaptic newHaptic)
		{
			Debug.Log("Add new haptic disabled\n");
			//recordedHaptics.Add(new TimedSerializableHaptic(newHaptic, specificTime));
		}

		public void SortElements()
		{
			recordedHaptics = recordedHaptics.OrderBy(x => x.PlayTime).ToList();
		}

		IEnumerator Playback(List<TimedSerializableHaptic> haptics)
		{
			float lastPlayTime = 0.0f;
			for (int i = 0; i < haptics.Count; i++)
			{
				float timeTillNextPlaytime = haptics[i].PlayTime - lastPlayTime;
				lastPlayTime = haptics[i].PlayTime;
				yield return new WaitForSeconds(timeTillNextPlaytime);

				var h = haptics[i].haptic;
				if (h.GetType() == typeof(HapticPattern))
				{
					((HapticPattern)h).Play();
				}
				if (h.GetType() == typeof(HapticExperience))
				{
					((HapticExperience)h).Play();
				}
			}
		}
	}
}