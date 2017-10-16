﻿using UnityEditor;

namespace Hardlight.SDK.Editor
{
	public class HelpMessage
	{
		public HelpMessage(string message, MessageType messageType)
		{
			this.message = message;
			this.messageType = messageType;
		}
		public string message;
		public MessageType messageType;
	}
}