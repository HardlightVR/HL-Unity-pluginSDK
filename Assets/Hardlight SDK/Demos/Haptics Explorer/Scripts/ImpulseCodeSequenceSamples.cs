using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	public class ImpulseHapticSequenceSamples : MonoBehaviour
	{
		public static HapticSequence ClickHum()
		{
			HapticSequence seq = HapticSequence.CreateNew();
			seq.AddEffect(Effect.Click);
			seq.AddEffect(Effect.Hum, .2f, .15f, 0.5f);
			return seq;
		}

		public static HapticSequence ThockClunk()
		{
			HapticSequence seq = HapticSequence.CreateNew();
			seq.AddEffect(Effect.Click, 0.15f);
			seq.AddEffect(Effect.Fuzz, .2f, .15f);
			return seq;
		}

		public static HapticSequence ClickStorm()
		{
			HapticSequence seq = HapticSequence.CreateNew();

			seq.AddEffect(Effect.Double_Click, 0);

			seq.AddEffect(Effect.Click, 0.1f);
			seq.AddEffect(Effect.Click, 0.2f);

			seq.AddEffect(Effect.Double_Click, 0.3f);

			seq.AddEffect(Effect.Triple_Click, 0.4f);

			seq.AddEffect(Effect.Double_Click, 0.5f);

			seq.AddEffect(Effect.Click, 0.6f);

			seq.AddEffect(Effect.Triple_Click, 0.7f);

			return seq;
		}

		public static HapticSequence DoubleClickImpact()
		{
			HapticSequence seq = HapticSequence.CreateNew();

			seq.AddEffect(Effect.Double_Click, 0);
			seq.AddEffect(Effect.Buzz, .05f, .05f);
			seq.AddEffect(Effect.Buzz, .1f, .1f, 0.6f);
			seq.AddEffect(Effect.Buzz, .15f, .2f, 0.2f);

			return seq;
		}

		public static HapticSequence Shimmer()
		{
			HapticSequence seq = HapticSequence.CreateNew();

			//Todo: new api, now we do have cross use
			seq.AddEffect(Effect.Double_Click, 0, 0, .5f);
			seq.AddEffect(Effect.Hum, .15f, 0.1f, .1f);
			seq.AddEffect(Effect.Hum, .25f, .5f, .2f);

			return seq;
		}
		//todo: reimplement all these as assets
		public static HapticSequence ClickHumDoubleClick()
		{
			HapticSequence seq = HapticSequence.CreateNew();

			seq.AddEffect(Effect.Click, 0);

			seq.AddEffect(Effect.Hum, 0.10f, .25f);

			seq.AddEffect(Effect.Double_Click, 0.6f);

			return seq;
		}

		public static HapticSequence PulseBumpPulse()
		{
			HapticSequence seq = HapticSequence.CreateNew();

			seq.AddEffect(Effect.Pulse, 0.0f, 0.7f);

			seq.AddEffect(Effect.Bump, 0.40f);

			seq.AddEffect(Effect.Pulse, 0.55f, 0.2f);

			return seq;
		}

		public static HapticSequence TripleClickFuzzFalloff()
		{
			HapticSequence seq = HapticSequence.CreateNew();

			seq.AddEffect(Effect.Triple_Click, 0.0f, 0.7f);

			seq.AddEffect(Effect.Fuzz, 0.2f);

			seq.AddEffect(Effect.Fuzz, 0.4f, 0.5f);

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

			HapticSequence seq = HapticSequence.CreateNew();

			float dur = ((float)rand.Next(0, 15)) / 10;
			float delay = ((float)rand.Next(0, 10)) / 20;
			seq.AddEffect(Effect.Pulse, dur, 0);
			float offset = dur;

			dur = ((float)rand.Next(0, 15)) / 20;
			delay = ((float)rand.Next(0, 8)) / 20;
			//Debug.Log(dur + "\n");
			seq.AddEffect(Effect.Pulse, dur, offset + delay);
			offset = dur;

			dur = ((float)rand.Next(0, 15)) / 20;
			delay = ((float)rand.Next(0, 8)) / 20;
			//Debug.Log(dur + "\n");
			seq.AddEffect(Effect.Pulse, dur, offset + delay);

			return seq;
		}


		/// <summary>
		/// Creating a randomized code sequence is totally doable.
		/// This is a less than ideal approach (because static method)
		/// In your code you shouldn't use a static method like this (Do as I say, not as I do)
		/// This one is about picking three Effect.ects at random (with random strength levels as well)
		/// </summary>
		/// <param name="randSeed">Hand in a random seed (or better yet, don't use random in static functions</param>
		/// <returns>A HapticSequence reference for use in Impulses</returns>
		public static HapticSequence ThreeRandomEffects(int randSeed)
		{
			//Debug.Log(randSeed + "\n");
			System.Random rand = new System.Random(randSeed);

			HapticSequence seq = HapticSequence.CreateNew();

			int Index = rand.Next(0, SuitImpulseDemo.effectOptions.Length);

			var eff = SuitImpulseDemo.effectOptions[Index];
			seq.AddEffect(eff, ((float)rand.Next(2, 10)) / 10, 0.0f);

			eff = SuitImpulseDemo.effectOptions[Index];
			seq.AddEffect(eff, ((float)rand.Next(2, 10)) / 10, .20f);

			eff = SuitImpulseDemo.effectOptions[Index];
			seq.AddEffect(eff, ((float)rand.Next(2, 10)) / 10, .4f);

			return seq;
		}


		/// <summary>
		/// A VERY random Effect.ect. More just for showing haptic varion
		/// </summary>
		/// <param name="randSeed"></param>
		/// <returns></returns>
		public static HapticSequence VeryRandomEffect(int randSeed)
		{
			//Debug.Log(randSeed + "\n");
			System.Random rand = new System.Random(randSeed);

			HapticSequence seq = HapticSequence.CreateNew();

			int Index = rand.Next(0, SuitImpulseDemo.effectOptions.Length);

			var eff = SuitImpulseDemo.effectOptions[Index];

			float dur = ((float)rand.Next(0, 6)) / 3;
			float delay = 0;
			float offset = 0;

			int HowManyEffects = rand.Next(2, 11);
			//Debug.Log("How many Effect.ects: " + HowManyEffects + "\n");
			for (int i = 0; i < HowManyEffects; i++)
			{
				Index = rand.Next(0, SuitImpulseDemo.effectOptions.Length);
				dur = ((float)rand.Next(0, 6)) / 3;
				delay = ((float)rand.Next(0, 8)) / 20;
				eff = SuitImpulseDemo.effectOptions[Index];
				seq.AddEffect(eff, dur, offset + delay);
				offset = dur;
			}

			return seq;
		}
	}
}