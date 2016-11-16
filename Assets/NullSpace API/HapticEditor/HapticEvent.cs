/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NullSpace.API.Enums;

namespace NullSpace.SDK.Editor
{
	public class HapticEvent
	{
		public Duration DurationType
		{
			get
			{
				if (this.duration > 0)
				{
					return Duration.Variable;
				}
				return (Duration)this.duration;
			}
		}
		public int duration;
		public string effect;
		public Effect Effect
		{
			get { return Effect.Buzz_100; }
		}
		public Location location;


		public HapticEvent(string e, Location l, int duration)
		{
			this.duration = Math.Max(duration, -1);
			this.effect = e;
			this.location = l;
		}

		override public string ToString()
		{
			return string.Format("HapticEvent: Duration = {0}, effect = {1}, location = {2}", this.DurationType, this.effect, this.location);
		}

	}
}
