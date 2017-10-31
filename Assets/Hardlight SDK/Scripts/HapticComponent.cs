using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hardlight.SDK
{
	[System.Serializable]
	public class HapticEvent : UnityEngine.Events.UnityEvent<SideOfHaptic> { }

	public enum SideOfHaptic
	{
		None = 0,
		Right = 1,
		Left = 2//,
				//Both = 3
	}

	[System.Serializable]
	public class HapticInfo
	{
		public enum PlayableType { Sequence, Pattern, Experience, Impulse }
		public enum ImpulseType { None, Emanation, Traversal }
		public PlayableType TypeOfPlayable;

		[ConditionalHide("TypeOfPlayable", true, "Impulse")]
		public string PlayableResourceName = "Haptics/click";

		[ConditionalHide("TypeOfPlayable", "Sequence")]
		//[RegionFlag]
		public AreaFlag Where;

		//[Header("Impulse Attributes")]
		[ConditionalHide("TypeOfPlayable", "Impulse")]
		public ImpulseType TypeOfImpulse = ImpulseType.None;

		[ConditionalHide("TypeOfImpulse", true, "None")]
		public string sequence = "Haptics/click";
		[ConditionalHide("TypeOfImpulse", true, "None")]
		public float Duration = .5f;

		[ConditionalHide("TypeOfImpulse", true, "None")]
		//[Range(-3, 3)]
		public float attenuationPercentage = 1;

		[ConditionalHide("TypeOfImpulse", true, "None")]
		public AreaFlag StartLocation;
		[ConditionalHide("TypeOfImpulse", "Traversal")]
		public AreaFlag EndLocation;


		[ConditionalHide("TypeOfImpulse", "Emanation")]
		//[Range(1, 8)]
		public int depth = 3;

		public HapticHandle TryToPlayPlay(SideOfHaptic side)
		{
			if (CanPlay(side))
			{
				var handle = CreateHandle(side);
				if (handle != null)
				{
					handle.Play();
					return handle;
				}
			}
			return null;
		}

		public bool CanPlay(SideOfHaptic side)
		{
			//Debug.Log("Can Play: " + side.ToString() + "\n");
			if (side == SideOfHaptic.None)
				return false;

			if (TypeOfPlayable == PlayableType.Sequence)
			{
				return PlayableResourceName.Length > 0 && Where != AreaFlag.None;
			}
			else if (TypeOfPlayable == PlayableType.Pattern)
			{
				return PlayableResourceName.Length > 0;
			}
			else if (TypeOfPlayable == PlayableType.Experience)
			{
				return PlayableResourceName.Length > 0;
			}
			else if (TypeOfPlayable == PlayableType.Impulse)
			{
				if (TypeOfImpulse == ImpulseType.None)
					return false;
				else if (TypeOfImpulse == ImpulseType.Emanation)
				{
					return sequence.Length > 0 && StartLocation != AreaFlag.None;
				}
				else if (TypeOfImpulse == ImpulseType.Traversal)
				{
					return sequence.Length > 0 && StartLocation != AreaFlag.None && EndLocation != AreaFlag.None;
				}
			}
			return true;
		}

		private HapticHandle CreateHandle(SideOfHaptic side)
		{
			HapticHandle handle = null;
			
			if (TypeOfPlayable == PlayableType.Sequence)
			{
				HapticSequence seq = HapticSequence.LoadFromAsset(PlayableResourceName);

				var areaFlag = side == SideOfHaptic.Left ? Where.Mirror() : Where;
				handle = seq.CreateHandle(areaFlag);
			}
			else if (TypeOfPlayable == PlayableType.Pattern)
			{
				HapticPattern pat = HapticPattern.LoadFromAsset(PlayableResourceName);
				handle = pat.CreateHandle();
			}
			else if (TypeOfPlayable == PlayableType.Experience)
			{
				HapticExperience exp = HapticExperience.LoadFromAsset(PlayableResourceName);
				handle = exp.CreateHandle();
			}
			else if (TypeOfPlayable == PlayableType.Impulse)
			{
				return CreateImpulseHandle(side);
			}

			return handle;
		}

		private HapticHandle CreateImpulseHandle(SideOfHaptic side)
		{
			if (TypeOfImpulse == ImpulseType.None)
				return null;
			else if (TypeOfImpulse == ImpulseType.Emanation)
			{
				bool mirror = side == SideOfHaptic.Left;
				var impulse = ImpulseGenerator.BeginEmanatingEffect(mirror ? StartLocation.Mirror() : StartLocation, depth);

				HapticSequence seq = HapticSequence.LoadFromAsset(sequence);

				impulse.WithEffect(seq).WithAttenuation(attenuationPercentage).WithDuration(Duration);

				return impulse.Play();

			}
			else if (TypeOfImpulse == ImpulseType.Traversal)
			{
				bool mirror = side == SideOfHaptic.Left;
				var impulse = ImpulseGenerator.BeginTraversingImpulse(
					mirror ? StartLocation.Mirror() : StartLocation,
					mirror ? EndLocation.Mirror() : EndLocation);

				HapticSequence seq = HapticSequence.LoadFromAsset(sequence);

				impulse.WithEffect(seq).WithAttenuation(attenuationPercentage).WithDuration(Duration);

				return impulse.Play();
			}
			return null;
		}
	}

	public class HapticComponent : MonoBehaviour
	{
		public bool ShouldPlayImmediately = false;

		public float MinDelayBetweenRepeatedPlays = .2f;
		public float MaxDelayBetweenRepeatedPlays = .65f;
		private float timeSinceLastPlay = 0.0f;
		private bool ReadyToPlay = true;
		[SerializeField]
		public HapticInfo MyHaptic;

		private HapticHandle handle;
		public void Play()
		{
			if (ReadyToPlay)
			{
				handle = MyHaptic.TryToPlayPlay(SideOfHaptic.Right);
				ResetDelayBetweenPlay();
			}
		}
		public void Play(SideOfHaptic side)
		{
			if (ReadyToPlay)
			{
				//Debug.Log(side.ToString() + "\n", this);
				handle = MyHaptic.TryToPlayPlay(side);
				ResetDelayBetweenPlay();
			}
		}
		public void StopIfPlaying()
		{
			if (handle != null)
			{
				handle.Stop();
			}
		}
		public void Restart()
		{
			if (handle == null)
			{
				Play();
			}
			else
			{
				handle.Replay();
			}
		}
		private void ResetDelayBetweenPlay()
		{
			timeSinceLastPlay = Random.Range(MinDelayBetweenRepeatedPlays, MaxDelayBetweenRepeatedPlays);
			ReadyToPlay = false;
		}
		void Update()
		{
			if (timeSinceLastPlay > 0)
			{
				ReadyToPlay = false;
				timeSinceLastPlay -= Time.deltaTime;
				if (timeSinceLastPlay <= 0)
				{
					ReadyToPlay = true;
				}
			}

			if (ShouldPlayImmediately)
			{
				ShouldPlayImmediately = false;
				Play(SideOfHaptic.Right);
			}
		}
	}
}