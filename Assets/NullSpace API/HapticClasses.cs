/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using NullSpace.API.Enums;

namespace NullSpace.SDK.Haptics
{
    public interface ITimeOffset
    {
        float time
        {
            get; set;
        }
    }
  
    /// <summary>
    /// Represents a list of haptic effects. In haptic files, a sequence takes place on one motor. In practice,
    /// a sequence can run on different motors if created programmatically.
    /// </summary>
    /// 
    public class HapticSequence
    {
        public List<HapticEffect> effects;
        public HapticSequence(List<HapticEffect> effects)
        {
            this.effects = effects;
        }
    }

    /// <summary>
    /// Represents a haptic effect, with waveform, duration, offset time, and priority.
    /// </summary>
    public class HapticEffect : IComparable, ITimeOffset
    {
        public Effect effect;
        public Location location;
        public float duration;
        public float time { get; set; }
        public float originalTime;
        public int priority;

        /// <summary>
        /// Construct a new HapticEffect
        /// </summary>
        /// <param name="effect">The effect</param>
        /// <param name="location">The location</param>
        /// <param name="duration">The duration (in seconds)</param>
        /// <param name="time">The time offset (in seconds)</param>
        /// <param name="priority">The priority (higher number is higher priority)</param>
        public HapticEffect(Effect effect, Location location, float duration, float time, int priority)
        {
            this.effect = effect;
            this.location = location;
            this.duration = duration;
            this.time = time;
            this.originalTime = time;
            this.priority = priority;
        }
        /*
        /// <summary>
        /// Construct a new HapticEffect, with default priority 1
        /// </summary>
        /// <param name="effect">The effect</param>
        /// <param name="location">The location</param>
        /// <param name="duration">The duration (in seconds)</param>
        /// <param name="time">The time offset (in seconds)</param>
        public HapticEffect(Effect effect, Location location, float duration, float time)
        {
            this.effect = effect;
            this.location = location;
            this.duration = duration;
            this.time = time;
            this.priority = 1;
        }
        /// <summary>
        /// Construct a new HapticEffect, with default priority 1, time offset 0.0
        /// </summary>
        /// <param name="effect">The effect</param>
        /// <param name="location">The location</param>
        /// <param name="duration">The duration (in seconds)</param>
        public HapticEffect(Effect effect, Location location, float duration)
        {
            this.effect = effect;
            this.location = location;
            this.duration = duration;
            this.time = 0.0f;
            this.priority = 1;
        }
        /// <summary>
        /// Construct a new HapticEffect, with default priority 1, time offset 0.0, duration 0.0
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="location"></param>
        public HapticEffect(Effect effect, Location location)
        {
            this.effect = effect;
            this.location = location;
            this.duration = 0.0f;
            this.time = 0.0f;
            this.priority = 1;
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        */
        public HapticEffect()
		{

		}

		public int CompareTo(object obj)
		{
			return time.CompareTo(((HapticEffect)obj).time);
		}
	}

	/// <summary>
	/// Represents a haptic frame. A frame contains a list of seperate haptic sequences to be kicked off
	/// at a certain time offset.
	/// </summary>
	public class HapticFrame : IComparable, ITimeOffset
	{
		public float time { get; set; }
        public float originalTime;
		public int priority;
		public List<HapticSequence> frame;

		/// <summary>
		/// Construct a new HapticFrame
		/// </summary>
		/// <param name="time">Time offset (in seconds)</param>
		/// <param name="frame">List of sequences that constitute the frame</param>
		/// <param name="priority">Priority (higher number is higher priority)</param>
		public HapticFrame(float time, List<HapticSequence> frame, int priority = 1)
		{
			this.time = time;
			this.priority = priority;
			this.frame = frame;
            this.originalTime = time;
		}

		public int CompareTo(object obj)
		{
			return time.CompareTo(((HapticFrame)obj).time);
		}
	}
   
	/// <summary>
	/// Represents a haptic sample. A sample consists of multiple frames, and is used in haptic experience files. Its main purpose
	/// is to enable time offsets of haptic patterns.
	/// </summary>
	public class HapticSample : IComparable, ITimeOffset
	{
		public int priority;
        public float time { get; set; }
        public float originalTime;
		public List<HapticFrame> frames;

		/// <summary>
		/// Construct a new HapticSample
		/// </summary>
		/// <param name="time">Time offset (in seconds)</param>
		/// <param name="frames">The frames which constitute the sample</param>
		/// <param name="priority">The priority (higher number is higher priority)</param>
		public HapticSample(float time, List<HapticFrame> frames, int priority)
		{
			this.frames = frames;
			this.time = time;
			//Necessary for the pqueue implementation
			this.priority = (priority * 100) + 1000;
            this.originalTime = time;
		}

		public int CompareTo(object obj)
		{
			return time.CompareTo(((HapticSample)obj).time);
		}
	}

}
