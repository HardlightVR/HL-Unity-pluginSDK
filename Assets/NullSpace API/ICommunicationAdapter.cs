/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections;

namespace NullSpace.API {
    /// <summary>
    /// Communication adapters must implement these methods to interface with the API
    /// </summary>
	public interface ICommunicationAdapter {
        /// <summary>
        /// Connect to the suit
        /// </summary>
        /// <returns>True on successful connection</returns>
		bool Connect ();
        /// <summary>
        /// Disconnect from the suit
        /// </summary>
		void Disconnect();
        /// <summary>
        /// Write bytes to the suit
        /// </summary>
        /// <param name="bytes">The raw bytes to write</param>
		void Write (byte[] bytes);
        /// <summary>
        /// Read from the suit into the buffer
        /// </summary>
		void Read();
	
	    /// <summary>
        /// The internal ring buffer containing suit packets
        /// </summary>
		ByteQueue suitDataStream
		{
			get;
		}

        /// <summary>
        /// Connectivity status of the suit
        /// </summary>
		bool IsConnected { get; }
	}
}
	


