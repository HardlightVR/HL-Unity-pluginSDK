/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using LitJson;
using System.Collections;

namespace NullSpace.API
{
    /// <summary>
    /// An internal builder which verifies hardware instructions against valid feedback zones, effects, and parameters
    /// </summary>
	public class InstructionBuilder
	{
		//We want to be able to retrieve an instruction by its name
		private IDictionary<string, Instruction> instructions;


		//Some state: the current instruction that the user desires to build
		private string instruction;

		//More state: the current parameter set in form {key : value}
		private IDictionary<string, string> parameters;

		//Handle indirection of a parameter key to the correct dynamically created map
		private IDictionary<string, Dictionary<string, byte>> paramDict;

		//We will populate this with possible parameters
		private string[] validParams;

		public InstructionBuilder()
		{
			paramDict = new Dictionary<string, Dictionary<string, byte>>();
			instructions = new Dictionary<string, Instruction>();
			parameters = new Dictionary<string, string>();
		

			validParams = new string[] { "zone", "effect", "data", "register" };
			foreach (string param in validParams)
			{
				paramDict[param] = new Dictionary<string, byte>();
			}

			 
		}

		public string GetDebugString()
		{
			string description = this.instruction + ": ";
			int index = 0;
			foreach (var param in this.parameters)
			{
				index++;
				description += param.Key + " = " + param.Value;
				if (index < this.parameters.Count)
				{
					description += ", ";
				}
			}
			return description;
		}

		// ---- Developers! Don't use this.  ----- //
        // ---- This is an internal API which is subject to change ---- //
		/// <summary>
		/// Select an instruction to build.
		/// </summary>
		/// <param name="name">Name of the instruction; instructions are found in the Instructions.json file</param>
		/// <returns></returns>
		public InstructionBuilder UseInstruction(string name)
		{
			this.parameters.Clear();
			this.instruction = name;
			return this;
		}
		/// <summary>
		/// Add a parameter to the selected instruction.
		/// </summary>
		/// <param name="key">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <returns></returns>
		public InstructionBuilder WithParam(string key, Enum value)
		{
			parameters[key] = value.ToString();
			return this;
		}
		/// <summary>
		/// Add a parameter to the selected instruction.
		/// </summary>
		/// <param name="key">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <returns></returns>
		public InstructionBuilder WithParam(string key, string value)
		{
			parameters[key] = value;
			return this;
		}
		/// <summary>
		/// Returns true if the instruction and parameter combination is valid; false otherwise
		/// </summary>
		/// <returns></returns>
		public bool Verify()
		{
			if (!instructions.ContainsKey(this.instruction))
			{
				return false;
			}

			//Does it have the required parameters?
			Instruction desired = instructions[this.instruction];

			foreach (string param in desired.parameters)
			{
				if (!this.parameters.ContainsKey(param))
				{
					return false;
				}

				//If we made it this far, the user has a {key : val} pair. 
				//We need to know if the val is valid.
                var dict = paramDict[param];
				if (!dict.ContainsKey(this.parameters[param])) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Builds a packet constructed with previous UseInstruction and WithParam calls. 
		/// </summary>
		/// <returns></returns>
		public byte[] Build()
		{
			Instruction desired = instructions[this.instruction];
			
			//7 = 4 (header len) + 3 (footer len) 
			int packetLength = 7 + this.parameters.Count;
			byte[] packet = new byte[packetLength];
			
			//Populate the header
			packet[0] = 0x24;
			packet[1] = 0x02;
			packet[2] = desired.ByteId;
			packet[3] = (byte) packetLength;
			
			//Populate the parameters
			for (int i = 0; i < this.parameters.Count; i++)
			{
				//Grab the Nth parameter (order matters, and is preserved in the json)
				string paramKey = desired.parameters[i];
				//Using it as a key, grab the value that the user set
				string userParamVal = this.parameters[paramKey];
				//Get the correct dictionary in which to lookup the byte id of the parameter
				var paramKeyToByteId = paramDict[paramKey];
				//Finally, retrieve the ID
				byte id = paramKeyToByteId[userParamVal];
				packet[i + 4] = id;
			}

			//Populate the footer
			packet[packetLength - 3] = 0xFF;
			packet[packetLength - 2] = 0xFF;
			packet[packetLength - 1] = 0x0A;
			return packet;
		}

        /// <summary>
        /// Load the feedback zones file
        /// </summary>
        /// <param name="json">A string of JSON containing zone definitions</param>
        /// <returns></returns>
        public bool LoadZones(string json)
        {
            return LoadKeyValue(paramDict["zone"], json);
        }
        /// <summary>
        /// Load the effects file
        /// </summary>
        /// <param name="json">A string of JSON containing effect definitions</param>
        /// <returns></returns>
		public bool LoadEffects(string json)
		{
			return LoadKeyValue(paramDict["effect"], json);
		}

        /// <summary>
        /// Load the instructions file
        /// </summary>
        /// <param name="json">A string of JSON containing the instructions</param>
        /// <returns></returns>
		public bool LoadInstructions(string json)
		{
			JsonData data = JsonMapper.ToObject(json);
			for (int i = 0; i < data.Count; i++)
			{
                Instruction test = JsonMapper.ToObject<Instruction>(new JsonReader(data[i].ToJson()));
				instructions[test.name] = test;
			}
			return true;
        }


		private bool LoadKeyValue(Dictionary<string, byte> dict, string json)
		{
			JsonData data = JsonMapper.ToObject(json);
			foreach (DictionaryEntry child in data)
			{
				byte value = Convert.ToByte(child.Value.ToString(), 16);
				dict[child.Key.ToString()] = value;
			}
			return true;
		}
	}
}
