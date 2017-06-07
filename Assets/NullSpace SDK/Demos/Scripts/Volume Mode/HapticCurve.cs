using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class HapticCurve : MonoBehaviour
	{
		[Header("The curve the haptic volume will follow")]
		public AnimationCurve MyCurve;

		[Range(0.1f, 5)]
		public float Duration = 0.5f;
		[Range(0.0f, 1)]
		public float minVolume = .1f;
		[Range(0.0f, 1)]
		public float maxVolume = .5f;

		private float counter = 0.0f;

		public KeyCode TriggerKey = KeyCode.None;
		[RegionFlag]
		public AreaFlag Where = AreaFlag.None;

		private bool Playing;
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
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(TriggerKey))
			{
				Stop();
			}

			if (StartPlaying && !Playing)
			{
				PlayRoutine = StartCoroutine(Play());
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
			AreaFlag[] areas = Where.ToArray();
			for (int i = 0; i < areas.Length; i++)
			{
				//Debug.Log("Silencing: " + areas[i].ToString() + "\n");
				NSManager.Instance.ControlDirectly(areas[i], 0);
			}
			for (int i = 0; i < areas.Length; i++)
			{
				//Debug.Log("Silencing: " + areas[i].ToString() + "\n");
				NSManager.Instance.ControlDirectly(areas[i], 0);
			}
			for (int i = 0; i < areas.Length; i++)
			{
				//Debug.Log("Silencing: " + areas[i].ToString() + "\n");
				NSManager.Instance.ControlDirectly(areas[i], 0);
			}
		}

		IEnumerator Play()
		{
			Playing = true;
			StartPlaying = false;
			float eval = 0.0f;
			float vol = 0.0f;
			counter = 0;
			Duration = Mathf.Clamp(Duration, 0, float.MaxValue);
			AreaFlag[] areas = Where.ToArray();

			while (counter <= Duration)
			{
				counter += Time.deltaTime;
				eval = MyCurve.Evaluate(counter / Duration);
				vol = minVolume + eval * (maxVolume - minVolume);
				//Debug.Log("Curve: " + eval + "\n" + vol);

				for (int i = 0; i < areas.Length; i++)
				{
					NSManager.Instance.ControlDirectly(areas[i], vol);
				}

				//Wait a frame
				yield return new WaitForSeconds(.01f);
			}
			Silence();
			Playing = false;
		}

		private void OnDestroy()
		{
			Silence();
		}

		public static HapticCurve CreateHapticCurve(AnimationCurve newCurve, float duration = .5f, float minVolume = .1f, float maxVolume = 1.0f)
		{
			//Make a new GameObject
			GameObject go = new GameObject();

			HapticCurve curve = go.AddComponent<HapticCurve>();


			return null;

		}
	}
}