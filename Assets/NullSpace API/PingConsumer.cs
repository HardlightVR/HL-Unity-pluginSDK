/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using NullSpace.API.Logger;
namespace NullSpace.API
{
    /// <summary>
    /// An extremely basic consumer which simply logs a message when it receives a ping from the suit
    /// </summary>
	class PingConsumer : PacketConsumer
	{
		public void ConsumePacket(byte[] packet)
		{
			//Log.Message("Received suit ping!");
		}
	}
}
