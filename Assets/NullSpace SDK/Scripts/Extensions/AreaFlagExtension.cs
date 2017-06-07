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
		public static AreaFlag[] AllAreas(this AreaFlag baseFlag)
		{
			return StaticAreaFlag;
		}
		public static AreaFlag[] ToArray(this AreaFlag baseFlag)
		{
			AreaFlag[] values = baseFlag.AllAreas();

			List<AreaFlag> has = new List<AreaFlag>();
			for (int i = 0; i < values.Length; i++)
			{
				if (baseFlag.HasFlag(values[i]))
				{
					has.Add(values[i]);
				}
			}

			return has.ToArray();
		}
		//public static bool HasFlag(this AreaFlag baseFlag, AreaFlag flag)
		//{
		//	if ((int)baseFlag & (int)flag) > 0)
		//		return true;
		//	if (((int)baseFlag & (int)flag)) == 1)
		//	{
		//		return true;
		//	}
		//	return HasFlag(baseFlag, flag);
		//}

		//		public static string CleanString(this AreaFlag baseFlag)
		//		{
		//			//			Remove tiny faces from the enum, they have no place here.
		//			string cleanedString = baseFlag.ToString().Replace('_', ' ');

		//			return cleanedString;
		//		}
	}
}