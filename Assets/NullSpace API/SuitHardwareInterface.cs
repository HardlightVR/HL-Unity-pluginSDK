/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

using NullSpace.API.Enums;
using NullSpace.API.Logger;

namespace NullSpace.API
{
	/// <summary>
	/// A direct interface to the underlying hardware
	/// </summary>
	public class SuitHardwareInterface
	{

		private ICommunicationAdapter adapter;
		public ICommunicationAdapter Adapter
		{
			get
			{
				return this.adapter;
			}
			set
			{
				if (this.adapter != null && this.adapter.IsConnected)
				{
					this.adapter.Disconnect();
				}
				this.adapter = value;
			}
		}

		private InstructionBuilder builder;
		private List<byte> deferredQueue;

		public SuitHardwareInterface()
		{
			builder = new InstructionBuilder();

			TextAsset instructions = (TextAsset)Resources.Load("NS Definitions/Instructions", typeof(TextAsset));
			TextAsset effects = (TextAsset)Resources.Load("NS Definitions/Effects", typeof(TextAsset));
			TextAsset zones = (TextAsset)Resources.Load("NS Definitions/Zones", typeof(TextAsset));

			builder.LoadInstructions(instructions.text);
			builder.LoadEffects(effects.text);
			builder.LoadZones(zones.text);

			deferredQueue = new List<byte>(2048);
		}


		public void ExecuteDeferred()
		{
			if (deferredQueue.Count > 0)
			{
				int buffSize = 64;
				if (deferredQueue.Count > buffSize)
				{
					//	Log.Message(deferredQueue.ToArray().Length);
					int iterations = deferredQueue.Count / buffSize;
					int remainder = deferredQueue.Count - (iterations * buffSize);
					for (int i = 0; i < iterations; i++)
					{
						var toEx = deferredQueue.GetRange(i * buffSize, buffSize).ToArray();
						this.Execute(toEx);
					}

					var toEx2 = deferredQueue.GetRange(iterations * buffSize, remainder).ToArray();

					this.Execute(toEx2);

				}
				else
				{
					this.Execute(deferredQueue.ToArray());
				}

				deferredQueue.Clear();
			}
		}

		public void ExecuteDeferredOld()
		{
			if (deferredQueue.Count > 0)
			{
				this.Execute(deferredQueue.ToArray());

				deferredQueue.Clear();
			}
		}

		/// <summary>
		/// Halt all effects playing on suit and shutdown
		/// </summary>
		public void Shutdown()
		{
			if (adapter != null)
			{
				///TODO: Bug
				this.HaltAllEffects();
				this.HaltAllEffects();

				adapter.Disconnect();
			}
		}

		/// <summary>
		/// Play a haptic effect once at a single location.
		/// </summary>
		/// <param name="location">Location on the suit</param>
		/// <param name="effect">Effect ID</param>
		public void PlayEffect(Location location, Effect effect)
		{
			if (builder.UseInstruction("PLAY_EFFECT")
				.WithParam("zone", location)
				.WithParam("effect", effect)
				.Verify())
			{
				this.Execute(builder.Build());
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}
		}
		public void PlayEffectDeferred(Location location, Effect effect)
		{
			if (builder.UseInstruction("PLAY_EFFECT")
				.WithParam("zone", location)
				.WithParam("effect", effect)
				.Verify())
			{
				this.deferredQueue.AddRange(builder.Build());
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}
		}
		/// <summary>
		/// Play a haptic effect continuously at a single location.
		/// </summary>
		/// <param name="location">Location on the suit</param>
		/// <param name="effect">Effect ID</param>
		public void PlayEffectContinuous(Location location, Effect effect)
		{
			if (builder.UseInstruction("PLAY_CONTINUOUS")
				.WithParam("effect", effect)
				.WithParam("zone", location)
				.Verify())
			{
				this.Execute(builder.Build());
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}
		}

		public void PlayEffectContinuousDeferred(Location location, Effect effect)
		{
			if (builder.UseInstruction("PLAY_CONTINUOUS")
							.WithParam("effect", effect)
							.WithParam("zone", location)
							.Verify())
			{
				this.deferredQueue.AddRange(builder.Build());
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}
		}

		/// <summary>
		/// Ping the suit. Allows the suit to shutdown if disconnected. 
		/// </summary>
		public void PingSuit()
		{
			if (builder.UseInstruction("STATUS_PING").Verify())
			{
				this.Execute(builder.Build());
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}

		}

		/// <summary>
		/// Halt a haptic effect at a single location.
		/// </summary>
		/// <param name="location">Location on the suit</param>
		public void HaltEffect(Location location)
		{
			if (builder.UseInstruction("HALT_SINGLE")
				.WithParam("zone", location)
				.Verify())
			{
				this.Execute(builder.Build());
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}

		}

		public void HaltEffectDeferred(Location location)
		{
			if (builder.UseInstruction("HALT_SINGLE")
				.WithParam("zone", location)
				.Verify())
			{
				this.deferredQueue.AddRange(builder.Build());
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}
		}

		/// <summary>
		/// Halt all haptic effects on the suit.
		/// </summary>
		public void HaltAllEffects()
		{
			foreach (Location e in Enum.GetValues(typeof(Location)))
			{
				if (e != Location.Error)
				{
					this.HaltEffect(e);
				}
			}
		}

		/// <summary>
		/// Fetch debug data for a single zone on the suit.
		/// </summary>
		/// <param name="location">Location on the suit</param>
		public void DebugZone(Location location)
		{
			if (!builder.UseInstruction("READ_DATA")
				.WithParam("zone", location)
				.Verify())
			{
				this.Execute(builder.Build());
				Log.Message("Retrieving debug info for zone {0}", location);
			}
			else
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
			}
		}


		/// <summary>
		/// Reset the suit by reinitializing all motors.
		/// </summary>
		public void ResetSuit()
		{
			Log.Message("Attempting to reset suit");
			if (!builder.UseInstruction("RESET_DRIVERS").Verify())
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
				return;
			}

			this.Execute(builder.Build());

			if (!builder.UseInstruction("INITIALIZE_ALL").Verify())
			{
				Log.Warning("Failed to build instruction {0}", builder.GetDebugString());
				return;
			}
			this.Execute(builder.Build());

		}

		private void Execute(byte[] rawPacket)
		{
			try
			{

				adapter.Write(rawPacket);
			}
			catch (IOException e)
			{
				Log.Error("Failed to write to the suit! {0}", e.Message);
			}
		}

		private void printByteArray(byte[] arr)
		{
			string formattedString = "";
			foreach (byte b in arr)
			{
				formattedString += b.ToString("X2") + " ";
			}
			Log.Message(formattedString);
		}

	}
}