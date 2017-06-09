using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class ImpulseHapticSequenceSamples : MonoBehaviour
	{
		public static HapticSequence ClickHum()
		{
			HapticSequence seq = new HapticSequence();
			HapticEffect eff = new HapticEffect(Effect.Click, 0.0f);
			seq.AddEffect(0, eff);
			eff = new HapticEffect(Effect.Hum, .2f);
			seq.AddEffect(.15, 0.5f, eff);
			return seq;
		}

		public static HapticSequence ThockClunk()
		{
			HapticSequence seq = new HapticSequence();
			HapticEffect eff = new HapticEffect(Effect.Click, 0.15f);
			seq.AddEffect(0, eff);
			eff = new HapticEffect(Effect.Fuzz, .2f);
			seq.AddEffect(.15, eff);
			return seq;
		}

		public static HapticSequence ClickStorm()
		{
			HapticSequence seq = new HapticSequence();

			HapticEffect eff = new HapticEffect(Effect.Double_Click, .0f);
			seq.AddEffect(0, eff);

			eff = new HapticEffect(Effect.Click, .0f);
			seq.AddEffect(0.1, eff);

			eff = new HapticEffect(Effect.Click, .0f);
			seq.AddEffect(0.2, eff);

			eff = new HapticEffect(Effect.Double_Click, .0f);
			seq.AddEffect(0.3, eff);

			eff = new HapticEffect(Effect.Triple_Click, .0f);
			seq.AddEffect(0.4, eff);

			eff = new HapticEffect(Effect.Double_Click, .0f);
			seq.AddEffect(0.5, eff);

			eff = new HapticEffect(Effect.Click, .0f);
			seq.AddEffect(0.6, eff);

			eff = new HapticEffect(Effect.Triple_Click, .0f);
			seq.AddEffect(0.7, eff);

			return seq;
		}

		public static HapticSequence DoubleClickImpact()
		{
			HapticSequence seq = new HapticSequence();
			HapticEffect eff = new HapticEffect(Effect.Double_Click, 0.00f);

			seq.AddEffect(0, eff);
			eff = new HapticEffect(Effect.Buzz, .05f);
			seq.AddEffect(.05, eff);
			eff = new HapticEffect(Effect.Buzz, .10f);
			seq.AddEffect(.10, 0.6f, eff);
			eff = new HapticEffect(Effect.Buzz, .15f);
			seq.AddEffect(.2, 0.2f, eff);

			return seq;
		}

		public static HapticSequence Shimmer()
		{
			HapticSequence seq = new HapticSequence();
			HapticEffect eff = new HapticEffect(Effect.Double_Click);

			//This is from the NS.DoD.Shimmer.sequence reimplemented as HapticSequence. this is because we don't yet have HapticSequence+File Sequence cross use.
			//{ "time" : 0.0, "effect" : "transition_hum", "strength" : 0.1, "duration" : 0.05},
			//{ "time" : 0.05, "effect" : Effect.Hum, "strength" : 0.1, "duration" : 0.1},
			//{ "time" : 0.15, "effect" : Effect.Hum, "strength" : 0.5, "duration" : 0.1},
			//{ "time" : 0.25, "effect" : Effect.Hum, "strength" : 0.1, "duration" : 0.1}

			//Todo: new api, now we do have cross use
		//	seq.AddEffect(0, eff);
			//eff = new HapticEffect("transition_hum", 0.05f, 0.1f);
			seq.AddEffect(.05, eff);
			eff = new HapticEffect(Effect.Hum, .1f);
			seq.AddEffect(.15, 0.1f, eff);
			eff = new HapticEffect(Effect.Hum, .1f);
			seq.AddEffect(.25,.5f, eff);

			return seq;
		}
		//todo: reimplement all these as assets
		public static HapticSequence ClickHumDoubleClick()
		{
			HapticSequence seq = new HapticSequence();

			HapticEffect eff = new HapticEffect(Effect.Click);
			seq.AddEffect(0, eff);

		//	eff = new HapticEffect("transition_hum", .50f, 1.0f);
			seq.AddEffect(0.10, eff);

			eff = new HapticEffect(Effect.Double_Click);
			seq.AddEffect(0.6, eff);

			return seq;
		}

		public static HapticSequence PulseBumpPulse()
		{
			HapticSequence seq = new HapticSequence();

			HapticEffect eff = new HapticEffect(Effect.Pulse, 0.40f);
			seq.AddEffect(0.0, 0.7f, eff);

			eff = new HapticEffect(Effect.Bump, .0f);
			seq.AddEffect(0.40, eff);

			eff = new HapticEffect(Effect.Pulse, 0.0f);
			seq.AddEffect(0.55, 0.2f, eff);

			return seq;
		}

		public static HapticSequence TripleClickFuzzFalloff()
		{
			HapticSequence seq = new HapticSequence();

			HapticEffect eff = new HapticEffect(Effect.Triple_Click, 0.20f);
			seq.AddEffect(0.0, 0.7f, eff);

			eff = new HapticEffect(Effect.Fuzz, .20f);
			seq.AddEffect(0.2, eff);

			eff = new HapticEffect(Effect.Fuzz, .20f);
			seq.AddEffect(0.4, 0.5f, eff);

			return seq;
		}

		/// <summary>
		/// Creating a randomized code sequence is totally doable.
		/// This is a less than ideal approach (because static method)
		/// In your code you shouldn't use a static method like this (Do as I say, not as I do)
		/// </summary>
		/// <param name="randSeed">Hand in a random seed (or better yet, don't use random in static functions</param>
		/// <returns>A HapticSequence reference for use in Impulses</returns>
		public static HapticSequence RandomPulses(int randSeed)
		{
			//Debug.Log(randSeed + "\n");
			System.Random rand = new System.Random(randSeed);

			HapticSequence seq = new HapticSequence();

			float dur = ((float)rand.Next(0, 15)) / 10;
			float delay = ((float)rand.Next(0, 10)) / 20;
			HapticEffect eff = new HapticEffect(Effect.Pulse, dur);
			seq.AddEffect(0.0, ((float)rand.Next(0, 10)) / 10, eff);
			float offset = dur;

			dur = ((float)rand.Next(0, 15)) / 20;
			delay = ((float)rand.Next(0, 8)) / 20;
			//Debug.Log(dur + "\n");
			eff = new HapticEffect(Effect.Pulse, dur);
			seq.AddEffect(offset + delay, ((float)rand.Next(0, 10)) / 10, eff);
			offset = dur;

			dur = ((float)rand.Next(0, 15)) / 20;
			delay = ((float)rand.Next(0, 8)) / 20;
			//Debug.Log(dur + "\n");
			eff = new HapticEffect(Effect.Pulse, dur);
			seq.AddEffect(offset + delay, ((float)rand.Next(0, 10)) / 10, eff);

			return seq;
		}


		/// <summary>
		/// Creating a randomized code sequence is totally doable.
		/// This is a less than ideal approach (because static method)
		/// In your code you shouldn't use a static method like this (Do as I say, not as I do)
		/// This one is about picking three effects at random (with random strength levels as well)
		/// </summary>
		/// <param name="randSeed">Hand in a random seed (or better yet, don't use random in static functions</param>
		/// <returns>A HapticSequence reference for use in Impulses</returns>
		public static HapticSequence ThreeRandomEffects(int randSeed)
		{
			//Debug.Log(randSeed + "\n");
			System.Random rand = new System.Random(randSeed);

			HapticSequence seq = new HapticSequence();

			int effIndex = rand.Next(0, SuitImpulseDemo.effectOptions.Length);

			HapticEffect eff = new HapticEffect(SuitImpulseDemo.effectOptions[effIndex], 0.0f);
			seq.AddEffect(0.0, ((float)rand.Next(2, 10)) / 10,eff);

			effIndex = rand.Next(0, SuitImpulseDemo.effectOptions.Length);
			eff = new HapticEffect(SuitImpulseDemo.effectOptions[effIndex], 0.0f);
			seq.AddEffect(.20f, ((float)rand.Next(2, 10)) / 10, eff);

			effIndex = rand.Next(0, SuitImpulseDemo.effectOptions.Length);
			eff = new HapticEffect(SuitImpulseDemo.effectOptions[effIndex], 0.0f);
			seq.AddEffect(.4f, ((float)rand.Next(2, 10)) / 10, eff);

			return seq;
		}


		/// <summary>
		/// A VERY random effect. More just for showing haptic varion
		/// </summary>
		/// <param name="randSeed"></param>
		/// <returns></returns>
		public static HapticSequence VeryRandomEffect(int randSeed)
		{
			//Debug.Log(randSeed + "\n");
			System.Random rand = new System.Random(randSeed);

			HapticSequence seq = new HapticSequence();

			int effIndex = rand.Next(0, SuitImpulseDemo.effectOptions.Length);

			float dur = ((float)rand.Next(0, 6)) / 3;
			float delay = 0;
			float offset = 0;
			HapticEffect eff = null;

			int HowManyEffects = rand.Next(2, 11);
			//Debug.Log("How many effects: " + HowManyEffects + "\n");
			for (int i = 0; i < HowManyEffects; i++)
			{
				effIndex = rand.Next(0, SuitImpulseDemo.effectOptions.Length);
				dur = ((float)rand.Next(0, 6)) / 3;
				delay = ((float)rand.Next(0, 8)) / 20;
				eff = new HapticEffect(SuitImpulseDemo.effectOptions[effIndex], dur);
				seq.AddEffect(offset + delay, ((float)rand.Next(0, 10)) / 10, eff);
				offset = dur;
			}

			return seq;
		}
	}
}