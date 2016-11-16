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

namespace NullSpace.API
{
    /// <summary>
    /// Represents a hardware instruction
    /// </summary>
	public class Instruction
	{
		private byte _id;
        /// <summary>
        /// ID of the instruction as a string
        /// </summary>
		public string id
		{
			get
			{
				return _id.ToString();
			}
			set
			{
				_id = Convert.ToByte(value, 16);
			}
		}
        /// <summary>
        /// ID of the instruction as a byte
        /// </summary>
		public byte ByteId
		{
			get
			{
				return _id;
			}
		}
		/// <summary>
        /// Human-readable name of the instruction
        /// </summary>
		public string name
		{
			get; set;
		}
        /// <summary>
        /// Purpose of the instruction
        /// </summary>
		public string purpose
		{
			get; set;
		}
        /// <summary>
        /// List of parameters associated with the instruction
        /// </summary>
		public string[] parameters
		{
			get; set;
		}
	}
}
