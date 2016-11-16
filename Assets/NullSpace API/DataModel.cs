/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using ca.axoninteractive.Collections;

using NullSpace.API;
using NullSpace.API.Enums;
using NullSpace.API.Logger;
using NullSpace.SDK.Haptics;

namespace NullSpace.SDK
{

	/// <summary>
	/// The default data model for interacting with the suit, which enables layering off haptic effects based on priority.
	/// This model also handles haptics played over durations. 
	/// </summary>
	public class DataModel : MonoBehaviour
	{
		/// <summary>
		/// Singleton instance of the model
		/// </summary>
		public static DataModel Instance;

		private IDictionary<Location, IHapticQueue> model;
		private SuitHardwareInterface suit;

		private List<TimeInstant> queuedFrames;
		private List<TimeInstant> queuedSamples;
		private List<TimeInstant> queuedEffects;
		private System.Object thisLock = new System.Object();


		public void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Log.Error("DataModel Instance is not null\nThere should only be one DataModel on the prefab object.");
			}

			//Each haptic zone will have a priority queue associated with it, to implement layering of effects
			model = new Dictionary<Location, IHapticQueue>();

			//Populate by looping through the Location enum
			foreach (Location loc in Enum.GetValues(typeof(Location)))
			{
				this.model[loc] = new HapticQueue();
			}

			queuedFrames = new List<TimeInstant>();
			queuedSamples = new List<TimeInstant>();
			queuedEffects = new List<TimeInstant>();
		}

		public void Start()
		{
			this.suit = NSManager.Instance.Suit;
		}

		/// <summary>
		/// Play a list of HapticSamples, equivalent to an experience 
		/// </summary>
		/// <param name="samples">List of samples</param>
		public void Play(List<HapticSample> samples)
		{
			lock (thisLock)
			{
				foreach (var s in samples)
				{
					this.queuedSamples.Add(new TimeInstant(0.0f, s));
				}
			}
		}


		/// <summary>
		/// Play a list of HapticEffects, equivalent to a sequence
		/// </summary>
		/// <param name="effects">List of effects</param>
		public void Play(List<HapticEffect> effects)
		{
			lock (thisLock)
			{
				foreach (var e in effects)
				{

					this.queuedEffects.Add(new TimeInstant(0.0f, e));
				}
			}
		}


		/// <summary>
		/// Play a list of HapticFrames, equivalent to a pattern
		/// </summary>
		/// <param name="frames">List of frames</param>
		public void Play(List<HapticFrame> frames)
		{
			lock (thisLock)
			{
				foreach (var f in frames)
				{

					this.queuedFrames.Add(new TimeInstant(0.0f, f));
				}
			}
		}


		public void Update()
		{

			//Loop through all feedback zones, update queues, send packets to suit
			updateLocations();
			lock (thisLock)
			{
				executePendingSamples();
				executePendingFrames();
				executePendingEffects();
			}
		}

		/// <summary>
		/// Remove all pending sequences, patterns, and experiences
		/// </summary>
		public void ClearAllPending()
		{
			this.queuedEffects.Clear();
			this.queuedFrames.Clear();
			this.queuedSamples.Clear();
		}

		/// <summary>
		/// Stops all currently playing effects
		/// </summary>
		public void ClearAllPlaying()
		{
			foreach (KeyValuePair<Location, IHapticQueue> queue in this.model)
			{
				this.model[queue.Key].Clear();
			}

			this.suit.HaltAllEffects();
			//TODO: Bug
			this.suit.HaltAllEffects();

		}

		private void executePendingSamples()
		{
			if (this.queuedSamples.Count == 0)
			{
				return;
			}

			for (int i = this.queuedSamples.Count - 1; i >= 0; i--)
			{
				this.queuedSamples[i].time += Time.deltaTime;
				//This completely buffers the sample's frames, disregarding the frame's offset time
				if (this.queuedSamples[i].Expired)
				{
					var hs = ((HapticSample)this.queuedSamples[i].item);
					foreach (var frame in hs.frames)
					{
						frame.priority = hs.priority;
						this.queuedFrames.Add(new TimeInstant(0.0f, frame));
					}
					this.queuedSamples.RemoveAt(i);
				}
			}
		}

		private void executePendingFrames()
		{
			if (this.queuedFrames.Count == 0)
			{
				return;
			}
			for (int i = this.queuedFrames.Count - 1; i >= 0; i--)
			{
				queuedFrames[i].time += Time.deltaTime;
				if (this.queuedFrames[i].Expired)
				{
					var hf = ((HapticFrame)queuedFrames[i].item);
					foreach (HapticSequence s in hf.frame)
					{
						foreach (HapticEffect e in s.effects)
						{
							e.priority = hf.priority;
							this.queuedEffects.Add(new TimeInstant(0.0f, e));
						}
					}
					this.queuedFrames.RemoveAt(i);
				}
			}
		}

		private void executePendingEffects()
		{
			if (this.queuedEffects.Count == 0)
			{
				return;
			}
			if (queuedEffects.Count == 1)
			{
				if (queuedEffects[0] == null)
				{
					Debug.Log("call casey: thread lock problems");
				}
				queuedEffects[0].time += Time.deltaTime;
				if (queuedEffects[0].Expired)
				{
					HapticEffect e = (HapticEffect)queuedEffects[0].item;
					model[e.location].Put(e.priority, new HapticEvent(e.effect, e.duration));

					queuedEffects.Clear();

				}
				return;
			}
			for (int i = queuedEffects.Count - 1; i >= 0; i--)
			{
				if (queuedEffects != null && queuedEffects[i] != null)
				{
					queuedEffects[i].time += Time.deltaTime;
					if (queuedEffects[i].Expired)
					{
						HapticEffect e = (HapticEffect)queuedEffects[i].item;
						model[e.location].Put(e.priority, new HapticEvent(e.effect, e.duration));

						queuedEffects.RemoveAt(i);

					}
				}
			}
		}

		private void updateLocations()
		{

			List<KeyValuePair<Duration, KeyValuePair<Location, Effect>>>
				toExecute = new List<KeyValuePair<Duration, KeyValuePair<Location, Effect>>>();

			foreach (KeyValuePair<Location, IHapticQueue> queue in this.model)
			{
				//Mark stuff which has expired as dirty
				model[queue.Key].Update(Time.deltaTime);
				//Grab top priority event from the model
				HapticEvent e = model[queue.Key].GetNextEvent();

				//The queue is empty?
				if (e == null)
				{
					//Then if, perhaps, some variable-duration effect just expired..
					if (queue.Value.dirty)
					{
						//Firmware has bug which requires two halts to stamp out edge case
						this.suit.HaltEffect(queue.Key);
						this.suit.HaltEffect(queue.Key);

						model[queue.Key].dirty = false;
					}
					continue;
				}

				toExecute.Add(new KeyValuePair<Duration, KeyValuePair<Location, Effect>>(
					e.DurationType,
							  new KeyValuePair<Location, Effect>(
					//the "key" information here ;-)
					queue.Key, e.effect
				)));

			}
			foreach (var kvp in toExecute)
			{
				var hapticEvent = kvp.Value;
				switch (kvp.Key)
				{
					case Duration.Infinite:
					case Duration.Variable:
						//TODO: Change back to deferred
						this.suit.PlayEffectContinuousDeferred(hapticEvent.Key, hapticEvent.Value);
						break;
					case Duration.OneShot:
						this.suit.HaltEffectDeferred(hapticEvent.Key);
						this.suit.PlayEffectDeferred(hapticEvent.Key, hapticEvent.Value);
						model[hapticEvent.Key].dirty = false;
						break;
					default:
						break;
				}
			}
			this.suit.ExecuteDeferred();
		}


		/// <summary>
		/// A haptic queue represents the layers of effects that might be playing on one location
		/// </summary>
		private interface IHapticQueue
		{
			/// <summary>
			/// Something expired in the queue
			/// </summary>
			bool dirty { get; set; }
			/// <summary>
			/// Put a new event in the queue
			/// </summary>
			/// <param name="priority">Priority of the event</param>
			/// <param name="e">The event</param>
			void Put(int priority, HapticEvent e);
			/// <summary>
			/// Update the queue
			/// </summary>
			/// <param name="deltaTime">Current delta time</param>
			void Update(float deltaTime);
			/// <summary>
			/// Return the highest priority event
			/// </summary>
			/// <returns>The highest priority event</returns>
			HapticEvent GetNextEvent();
			/// <summary>
			/// Removes all events in the queue
			/// </summary>
			void Clear();
		}

		/// <summary>
		/// Represents a list-based priority queue of haptic events at each haptic zone. The queue is not concerned with time delayed events;
		/// all events in the queue are currently executing.
		/// </summary>
		private class HapticQueue : IHapticQueue
		{

			private List<PriorityValuePair<HapticEvent>> queue;
			//Does some housekeeping need to be performed?
			public bool dirty { get; set; }

			public HapticQueue()
			{
				this.queue = new List<PriorityValuePair<HapticEvent>>(16);
				this.dirty = false;
			}
			private bool isHigherPriorityOneshot(HapticEvent b, PriorityValuePair<HapticEvent> a, int priority)
			{
				return a.Priority < priority && b.DurationType == Duration.OneShot;
			}
			public void Put(int priority, HapticEvent effect)
			{
				//if nothing in the queue, then we add
				if (queue.Count == 0)
				{
					this.queue.Add(new PriorityValuePair<HapticEvent>(priority, effect));

				}
				else
				//only add if it is a variable duration (because it might layer in background)
				//or if it is a oneshot with higher priority. A oneshot with lower priority than the top layer
				//should not be played.
				if (effect.DurationType != Duration.OneShot || isHigherPriorityOneshot(effect, queue[0], priority))
				{
					for (int i = 0; i < this.queue.Count; i++)
					{
						if (this.queue[i].Priority > priority)
						{
							i++;
						}
						else
						{
							this.queue.Insert(i, new PriorityValuePair<HapticEvent>(priority, effect));

							break;
						}
					}
					this.queue.Add(new PriorityValuePair<HapticEvent>(priority, effect));
					//this.queue.Insert(this.queue.Count, new PriorityValuePair<HapticEvent>(priority, effect));
				}
			}

			public void Update(float deltaTime)
			{
				foreach (PriorityValuePair<HapticEvent> e in this.queue)
				{
					if (e.Value.dirty)
					{
						//skip for now
						continue;
					}

					//good to go: move forward in time, or if expired, mark as dirty. Will sweep later.
					switch (e.Value.DurationType)
					{
						case Duration.Variable:
							if (e.Value.timeElapsed < e.Value.duration - deltaTime)
							{
								e.Value.timeElapsed += deltaTime;
							}
							else
							{
								e.Value.dirty = true;
								this.dirty = true;
							}
							continue;
						default:
							continue;
					}
				}
			}

			/// <summary>
			/// Remove old events
			/// </summary>
			private void Purge()
			{
				for (int i = this.queue.Count - 1; i >= 0; i--)
				{
					if (this.queue[i].Value.dirty)
					{
						this.queue.RemoveAt(i);
						continue;
					}

					if (this.queue[i].Priority < this.queue[0].Priority && this.queue[i].Value.DurationType == Duration.OneShot)
					{
						this.queue.RemoveAt(i);
						continue;
					}
				}
			}


			/// <summary>
			/// Retrieve and possibly dequeue the highest priority event
			/// </summary>
			/// <returns>the highest priority event</returns>
			public HapticEvent GetNextEvent()
			{
				this.Purge();

				if (this.queue.Count > 0)
				{
					HapticEvent h = this.queue[0].Value;
					if (h.DurationType == Duration.OneShot)
					{
						//only dequeue if it is a oneshot. Variable and continuous will be removed later. 
						this.queue.RemoveAt(0);
					}
					return h;
				}
				return null;
			}

			public void Clear()
			{
				this.queue.Clear();
				this.dirty = false;
			}
		}

		/// <summary>
		/// Represents a heap-based queue of haptic events
		/// </summary>
		private class PriorityHapticQueue : IHapticQueue
		{
			private ConcurrentPriorityQueue<HapticEvent> queue;
			//Does some housekeeping need to be performed?
			public bool dirty { get; set; }

			public PriorityHapticQueue()
			{
				this.queue = new ConcurrentPriorityQueue<HapticEvent>();
			}

			public void Put(int priority, HapticEvent effect)
			{
				this.queue.Add(new PriorityValuePair<HapticEvent>(priority, effect));
			}

			public void Update(float deltaTime)
			{
				foreach (PriorityValuePair<HapticEvent> e in this.queue)
				{
					if (e.Value.dirty)
					{
						//skip for now
						continue;
					}

					//good to go: move forward in time, or if expired, mark as dirty. Will sweep later.
					switch (e.Value.DurationType)
					{
						case Duration.Variable:
							if (e.Value.timeElapsed < e.Value.duration - deltaTime)
							{
								e.Value.timeElapsed += deltaTime;
							}
							else
							{
								e.Value.dirty = true;
								this.dirty = true;
							}
							continue;
						default:
							continue;
					}
				}
			}

			/// <summary>
			/// Remove old events
			/// </summary>
			public void Purge()
			{
				List<PriorityValuePair<HapticEvent>> events = new List<PriorityValuePair<HapticEvent>>();

				foreach (PriorityValuePair<HapticEvent> e in this.queue)
				{
					if (e.Value.dirty)
					{
						events.Add(e);
					}

					//If the effect is buried under higher priority effects, and it's a OneShot, then remove it
					//because it's time has forever passed :(
					if (e.Priority < this.queue.Peek().Priority && e.Value.DurationType == Duration.OneShot)
					{
						events.Add(e);
					}
				}

				foreach (var pair in events)
				{
					this.queue.Remove(pair);
				}
			}


			/// <summary>
			/// Retrieve and possibly dequeue the highest priority event
			/// </summary>
			/// <returns>the highest priority event</returns>
			public HapticEvent GetNextEvent()
			{
				this.Purge();

				if (this.queue.Count > 0)
				{
					HapticEvent h = this.queue.Peek().Value;
					if (h.DurationType == Duration.OneShot)
					{
						//only dequeue if it is a oneshot. Variable and continuous will be removed later. 
						this.queue.Dequeue();
					}
					return h;
				}
				return null;
			}

			public void Clear()
			{
				this.queue.Clear();
				this.dirty = false;
			}
		}

		/// <summary>
		/// Represents an event stored in the model's priority queues
		/// </summary>
		private class HapticEvent
		{
			//does the event need to be removed?
			public bool dirty;
			public Effect effect;
			public float duration;
			//how long the event has been active in the queue
			public float timeElapsed;
			public Duration DurationType
			{
				get
				{
					if (this.duration > 0f)
					{
						return Duration.Variable;
					}
					return (Duration)this.duration;
				}
			}

			public HapticEvent(Effect effect, float duration)
			{
				this.effect = effect;
				this.duration = duration;
				this.timeElapsed = 0f;
				this.dirty = false;
			}
		}

		private class TimeInstant : IComparable
		{
			public float time;
			public ITimeOffset item;
			public TimeInstant(float t, ITimeOffset to)
			{
				this.time = t;
				this.item = to;
			}

			public bool Expired
			{
				get { return this.time >= this.item.time; }
			}

			public int CompareTo(object o)
			{

				return time.CompareTo(((TimeInstant)o).time);
			}
		}
	}
}