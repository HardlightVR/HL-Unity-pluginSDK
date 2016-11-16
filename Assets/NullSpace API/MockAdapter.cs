/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullSpace.API
{
    /// <summary>
    /// A mock adapter for use when a physical suit is not present
    /// </summary>
	public class MockAdapter : ICommunicationAdapter
	{
		private ByteQueue mockData = new ByteQueue();

		public bool IsConnected
		{
			get
			{
				return true;
			}
		}

		public ByteQueue suitDataStream
		{
			get
			{
				return mockData;
			}
		}

		public bool Connect()
		{
			return true;
		}

		public void Disconnect()
		{
		}

		public void Read()
		{
		}

		public void Write(byte[] bytes)
		{
		}
	}
}
