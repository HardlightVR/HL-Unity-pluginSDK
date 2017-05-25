/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace NullSpace.SDK
{
	public static class ImpulseGenerator
	{
		public static GraphEngine _grapher = new GraphEngine();

		/// <summary>
		/// Begins an emanating impulse at the provided origin.
		/// Defaults to a simple 'hum' effect if you don't call WithEffect()
		/// Remember to call .Play() on the returned impulse.
		/// </summary>
		/// <param name="origin">The starting AreaFlag. Only provided a single AreaFlag pad.</param>
		/// <param name="depth">How many pads this will traverse before ending (will not reverb off 'end paths')</param>
		/// <returns>An Impulse object which can be given a HapticSequence, duration or Attenuation parameters
		/// Remember to call Play() on the returned object to begin the emanation
		/// <returns>The Impulse that you can call .Play() on to play a create a HapticHandle referencing that Haptic</returns>
		public static Impulse BeginEmanatingEffect(AreaFlag origin, int depth = 2)
		{
			if (!origin.IsSingleArea())
			{
				Debug.LogError("Invalid AreaFlag Provided: Origin is [" + origin.NumberOfAreas() + "] area(s).\n\tImpulse Generator only supports single area flag values.\n");
				return null;
			}

			if (depth < 0)
			{
				Debug.LogError("Depth for emanation is negative: " + depth + "\n\tThis will be clamped to 0 under the hood, negative numbers will likely do nothing");
			}

			CreateImpulse creation = delegate (float attenuation, float totalLength, HapticSequence seq)
			{
				HapticPattern emanation = new HapticPattern();
				var stages = _grapher.BFS(origin, depth);
				float baseStrength = 1.0f;
				float timeStep = totalLength / stages.Count;
				float time = 0.0f;
				for (int i = 0; i < stages.Count; i++)
				{
					AreaFlag area = AreaFlag.None;
					foreach (var item in stages[i])
					{
						area |= item.Location;
					}
					if (i > 0)
					{
						baseStrength *= (attenuation);
					}
					//Debug.Log(timeStep + "\t\t" + time + "   " + baseStrength + "\n");

					emanation.AddSequence(time, area, Mathf.Clamp(baseStrength, 0f, 1f), seq);
					time += timeStep;
				}

				return emanation.CreateHandle().Play();
			};

			return new Impulse(creation);
		}

		/// <summary>
		/// Begins a traversing impulse at the provided origin.
		/// Will play on pads that are the origin, destination or 'in-between' them.
		/// Defaults to a simple 'hum' effect if you don't call WithEffect()
		/// Remember to call .Play() on the returned impulse.
		/// </summary>
		/// <param name="origin">The starting AreaFlag. Only provided a single AreaFlag pad.</param>
		/// <param name="destination">The ending location for the traversing impulse.</param>
		/// <returns>The Impulse that you can call .Play() on to play a create a HapticHandle referencing that Haptic</returns>
		public static Impulse BeginTraversingImpulse(AreaFlag origin, AreaFlag destination)
		{
			if (!origin.IsSingleArea() || !destination.IsSingleArea())
			{
				Debug.LogError("Invalid AreaFlag Provided: Origin is [" + origin.NumberOfAreas() + "] area(s) and Destination is [" + destination.NumberOfAreas() + "] area(s).\n\tImpulse Generator only supports single area flag values.\n");
				return null;
			}
			CreateImpulse creation = delegate (float attenuation, float totalLength, HapticSequence seq)
			{
				HapticPattern emanation = new HapticPattern();
				var stages = _grapher.Dijkstras(origin, destination);

				float timeStep = totalLength / stages.Count;
				float time = 0.0f;
				float baseStrength = 1f;
				for (int i = 0; i < stages.Count; i++)
				{
					if (i > 0)
					{
						baseStrength *= (attenuation);
					}
					//Debug.Log(timeStep + "\n" + baseStrength + "\n");
					emanation.AddSequence(time, stages[i].Location, Mathf.Clamp(baseStrength, 0f, 1f), seq);
					time += timeStep;
				}

				return emanation.CreateHandle().Play();
			};

			return new Impulse(creation);
		}

		internal delegate HapticHandle CreateImpulse(float attenuation, float totalLength, HapticSequence seq);
		public class Impulse
		{
			private float totalLength;
			private float attenuation;
			private CreateImpulse process;
			private HapticSequence seq;

			/// <summary>
			/// Creates a new HapticSequence and overwrites this Impulse's current Sequence. 
			/// </summary>
			/// <param name="effect">The effect family to play.</param>
			/// <param name="duration">The duration of the sequence.</param>
			/// <param name="strength">The strength of the effect (which selects the corresponding family member under the hood)</param>
			/// <returns>The Impulse that you can call .Play() on to play a create a HapticHandle referencing that Haptic</returns>
			public Impulse WithEffect(Effect effect, float duration = 0.0f, float strength = 1.0f)
			{
				if (duration < 0.0f)
				{
					throw new ArgumentException("Attempted to assign a negative duration for Impulse's WithEffect(). How would that even work?", "duration");
				}
				if (strength < 0.0f)
				{
					Debug.LogWarning("[ImpulseGenerator] was provided a negative effect strength. Clamped to 0.0f");
					strength = 0.0f;
				}

				HapticSequence seq = new HapticSequence();
				seq.AddEffect(0.0f, strength, new HapticEffect(effect, duration));
				if (seq == null)
				{
					throw new ArgumentException("Attempted to assign a null HapticSequence seq - retaining previous HapticSequence", "seq");
				}
				this.seq = seq;
				return this;
			}

			/// <summary>
			/// Sets and overwrites this Impulse's current Sequence
			/// </summary>
			/// <param name="seq"></param>
			/// <returns></returns>
			public Impulse WithEffect(HapticSequence seq)
			{
				if (seq == null)
				{
					throw new ArgumentException("Attempted to assign a null HapticSequence seq - retaining previous HapticSequence", "seq");
				}
				this.seq = seq;
				return this;
			}

			/// <summary>
			/// Sets the duration of the entire impulse
			/// </summary>
			/// <param name="duration">A value greater than 0. Throws exception for negative numbers.</param>
			/// <returns>The Impulse that you can call .Play() on to play a create a HapticHandle referencing that Haptic</returns>
			public Impulse WithDuration(float duration)
			{
				if (duration < 0.0f)
				{
					throw new ArgumentException("Attempted to assign a negative duration for Impulse's WithDuration(). How would that even work?", "duration");
				}
				this.totalLength = duration;
				return this;
			}

			/// <summary>
			/// Changes the attenuation from pad to pad.
			/// A value less than 1 will decrease the strength with each pad.
			/// A value greater than 1 will increase the strength with each pad.
			/// A negative value will create weird behavior (which might get deprecated later).
			/// </summary>
			/// <param name="attenuationPercentage">A value of .0001 to 10. Can be like -1 but that might create weird behavior.</param>
			/// <returns>The Impulse that you can call .Play() on to play a create a HapticHandle referencing that Haptic</returns>
			public Impulse WithAttenuation(float attenuationPercentage)
			{
				this.attenuation = attenuationPercentage;
				return this;
			}

			/// <summary>
			/// Begins the Impulse. 
			/// Can be called multiple times for multiple HapticHandle instances of the effect.
			/// Think of an Impulse like robot machine where each robot knows the same song, but can each be playing it individually.
			/// </summary>
			/// <returns>The HapticHandle for resetting, pausing or stopping the haptic effect</returns>
			public HapticHandle Play()
			{
				if (seq == null)
				{
					throw new ArgumentException("Impulse's Sequence cannot be null - it must have been assigned an invalid HapticSequence earlier in the code flow", "seq");
				}
				else
				{
					return process(attenuation, totalLength, seq);
				}
			}

			/// <summary>
			/// Provide a HapticSequence and then Play the impulse.
			/// Each Play() creates a new HapticHandle instance for that individual played effect.
			/// A helper function for WithEffect(sequence).Play()
			/// </summary>
			/// <param name="sequence">A valid HapticSequence</param>
			/// <returns>The HapticHandle for resetting, pausing or stopping the haptic effect</returns>
			public HapticHandle Play(HapticSequence sequence)
			{
				return WithEffect(sequence).Play();
			}

			internal Impulse(CreateImpulse process)
			{
				this.process = process;
				this.attenuation = 1.0f;
				this.totalLength = 2.0f;
				WithEffect(Effect.Hum);
			}
		}

		//Future feature: Repeated Impulses.
		//They Play N+Reptition times
		//They delay by X seconds from each start.
		//public class RepeatedImpulse : Impulse
		//{
		//	private int repetitions = 1;
		//	private float delayOnRepetition = .2f;


		//	internal RepeatedImpulse(CreateImpulse process)
		//	{
		//		this.process = process;
		//		this.attenuation = 0.0f;
		//		this.totalLength = 2.0f;
		//		WithEffect(Effect.Hum);

		//	}
		//}

	}
}