using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	/// <summary>
	/// [Incomplete] This is an incomplete class for handling haptic curves.
	/// It still has some minor bugs. In other words...
	/// DO NOT USE.
	/// </summary>
	public class HapticCurve : MonoBehaviour
	{
		[Range(0.0f, 5, order = 1)]
		public float timeDelay = 0f;
		[Space(-10, order = 2)]

		[Header("The curve the haptic volume will follow", order = 3)]
		public AnimationCurve MyCurve;

		[Range(0.1f, 5)]
		public float Duration = 0.5f;
		[Range(0.0f, 5)]
		public float endingSustain = 0f;
		[Space(10)]
		[Range(0.0f, 1)]
		public float minVolume = .1f;
		[Range(0.0f, 1)]
		public float maxVolume = .5f;

		public KeyCode TriggerKey = KeyCode.None;

		[Space(10)]
		[RegionFlag]
		public AreaFlag Where = AreaFlag.None;

		public bool InMainCoroutineWhile = false;
		public bool PlaySustainAtEnd = true;

		private float counter = 0.0f;
		private bool _playing;
		private bool Playing
		{
			get { return _playing; }
			set
			{
				_playing = value;
			}

		}
		private bool StartPlaying;
		private Coroutine PlayRoutine;

		void Start()
		{
			//MyCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
			//MyCurve.preWrapMode = WrapMode.PingPong;
			//MyCurve.postWrapMode = WrapMode.PingPong;
		}
		
		void Update()
		{

			if (Input.GetKeyDown(TriggerKey))
			{
				StartPlaying = true;
			}

			if (Input.GetKeyDown(KeyCode.Home))
			{
				Stop();
			}

			if (StartPlaying && !Playing)
			{
				PlayRoutine = StartCoroutine(Play(timeDelay));
			}
		}

		public void Stop()
		{
			if (Playing)
			{
				StopCoroutine(PlayRoutine);
				Silence();
			}
		}

		public void Silence()
		{
			string hit = "";
			if (PlayRoutine != null)
			{
				hit += "Stopping\n";
				StopCoroutine(PlayRoutine);

			}
			Playing = false;
			InMainCoroutineWhile = false;
			counter = Duration;
			PlaySustainAtEnd = false;

			AreaFlag[] areas = Where.ToArray();
			for (int i = 0; i < areas.Length; i++)
			{
				hit += "Silencing: " + areas[i].ToString() + "\n";
				//Debug.Log("Silencing: " + areas[i].ToString() + "\n\t" + name);
				NSManager.Instance.ControlDirectly(areas[i], 0);
			}
			//for (int i = 0; i < areas.Length; i++)
			//{
			//	//Debug.Log("Silencing: " + areas[i].ToString() + "\n");
			//	NSManager.Instance.ControlDirectly(areas[i], 0);
			//}
			//for (int i = 0; i < areas.Length; i++)
			//{
			//	//Debug.Log("Silencing: " + areas[i].ToString() + "\n");
			//	NSManager.Instance.ControlDirectly(areas[i], 0);
			//}
			Debug.Log(hit);
		}

		IEnumerator Play(float delayBeforeStart = 0.0f)
		{
			InMainCoroutineWhile = false;
			PlaySustainAtEnd = true;
			if (delayBeforeStart > 0)
			{
				yield return new WaitForSeconds(delayBeforeStart);
			}

			Playing = true;
			StartPlaying = false;
			float eval = 0.0f;
			float vol = 0.0f;
			counter = 0;
			Duration = Mathf.Clamp(Duration, 0, float.MaxValue);
			AreaFlag[] areas = Where.ToArray();

			while (counter < Duration)
			{
				InMainCoroutineWhile = true;
				//Debug.Log("Curve: " + eval + "\n" + vol + "   " + Duration + "   " + counter);

				for (int i = 0; i < areas.Length; i++)
				{
					NSManager.Instance.ControlDirectly(areas[i], vol);
				}
				counter = Mathf.Clamp(counter + Time.deltaTime, 0.0f, Duration);
				eval = MyCurve.Evaluate(counter / Duration);
				vol = minVolume + eval * (maxVolume - minVolume);


				//Wait a frame
				yield return new WaitForSeconds(.01f);
			}

			if (endingSustain > 0 && PlaySustainAtEnd)
			{
				eval = MyCurve.Evaluate(1.0f);
				vol = minVolume + eval * (maxVolume - minVolume);

				for (int i = 0; i < areas.Length; i++)
				{
					NSManager.Instance.ControlDirectly(areas[i], vol);
				}
				yield return new WaitForSeconds(endingSustain);
			}

			Silence();
		}

		private void OnDestroy()
		{
			Silence();
		}

		//public static HapticCurve CreateHapticCurve(AnimationCurve newCurve, float duration = .5f, float minVolume = .1f, float maxVolume = 1.0f)
		//{
		//	//Make a new GameObject
		//	//GameObject go = new GameObject();

		//	//HapticCurve curve = go.AddComponent<HapticCurve>();


		//	return null;

		//}
	}
}