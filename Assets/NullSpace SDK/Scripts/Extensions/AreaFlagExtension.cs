using System.Linq;
using System.Collections.Generic;

namespace NullSpace.SDK
{
	public static class AreaFlagExtensions
	{
		//		//public static AreaFlag ConvertFlag(string flagName)
		//		//{
		//		//	return (AreaFlag)System.Enum.Parse(typeof(AreaFlag), flagName, true); ;
		//		//}
		//		//public static bool HasFlag(int baseFlag, int flag)
		//		//{
		//		//	if ((baseFlag & flag) == flag)
		//		//	{
		//		//		return true;
		//		//	}
		//		//	return false;
		//		//}

		public static AreaFlag[] StaticAreaFlag =
		{
			AreaFlag.Forearm_Left,
			AreaFlag.Upper_Arm_Left,
			AreaFlag.Shoulder_Left,
			AreaFlag.Back_Left,
			AreaFlag.Chest_Left,
			AreaFlag.Upper_Ab_Left,
			AreaFlag.Mid_Ab_Left,
			AreaFlag.Lower_Ab_Left,
			AreaFlag.Forearm_Right,
			AreaFlag.Upper_Arm_Right,
			AreaFlag.Shoulder_Right,
			AreaFlag.Back_Right,
			AreaFlag.Chest_Right,
			AreaFlag.Upper_Ab_Right,
			AreaFlag.Mid_Ab_Right,
			AreaFlag.Lower_Ab_Right
		};

		public static bool IsSingleArea(this AreaFlag baseFlag)
		{
			return baseFlag.NumberOfAreas() == 1;
		}
		public static int NumberOfAreas(this AreaFlag baseFlag)
		{
			//This is credited as the Hamming Weight, Popcount or Sideways Addition.
			//Source: Stack Overflow
			//https://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer

			//Really cool way to count the number of flags.

			int i = (int)baseFlag;
			// Java: use >>> instead of >>
			// C or C++: use uint32_t
			i = i - ((i >> 1) & 0x55555555);
			i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
			return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
		}
		public static AreaFlag AddFlag(this AreaFlag baseFlag, AreaFlag added)
		{
			baseFlag = baseFlag | added;
			return baseFlag;
		}
		public static bool HasFlag(this AreaFlag baseFlag, int flag)
		{
			if (((int)baseFlag & (flag)) == flag)
			{
				return true;
			}
			return false;
		}
		public static bool HasFlag(this AreaFlag baseFlag, AreaFlag checkFlag)
		{
			return HasFlag(baseFlag, (int)checkFlag);
		}
		/// <summary>
		/// This function does NOT break apart the AreaFlag it is called on. For that, call ToArray()
		/// Returns an array of ALL single AreaFlags in the enum.
		/// Does not include Boths, Alls or None
		/// </summary>
		/// <param name="baseFlag">This value does not matter at all. You cannot append extension methods to Enums.</param>
		/// <returns>Returns an array of each single area in the enum. (No Boths, Alls or None AreaFlags)</returns>
		public static AreaFlag[] AllSingleAreasInEnum(this AreaFlag baseFlag)
		{
			return StaticAreaFlag;
		}
		/// <summary>
		/// Breaks a multiple AreaFlag into a list of Single AreaFlags
		/// Ex: Multi Area Flag = AreaFlag.Forearm_Left|AreaFlag.Forearm_Right
		/// Return: {AreaFlag.Forearm_Left, AreaFlag.Forearm_Right
		/// </summary>
		/// <param name="baseFlag">The flag to be evaluated.</param>
		/// <returns>Retusn an array of single AreaFlags. Will not include None, Boths or All AreaFlag values.</returns>
		public static AreaFlag[] ToArray(this AreaFlag baseFlag)
		{
			AreaFlag[] values = baseFlag.AllSingleAreasInEnum();

			List<AreaFlag> has = new List<AreaFlag>();
			for (int i = 0; i < values.Length; i++)
			{
				if (baseFlag.HasFlag(values[i]))
				{
					has.Add(values[i]);
				}
			}

			//
			//if (has.Count < 1)
			//{
			//	has.Add(AreaFlag.None);
			//}

			return has.ToArray();
		}
	}
}