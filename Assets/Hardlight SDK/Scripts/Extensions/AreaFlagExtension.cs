using System.Linq;
using System.Collections.Generic;

namespace Hardlight.SDK
{
	public static class AreaFlagExtensions
	{
		public static AreaFlag Mirror(this AreaFlag lhs)
		{
			return (AreaFlag)RotateLeft((uint)lhs, 16);
		}
		/// <summary>
		/// Circular Bitshift Left
		/// </summary>
		/// <param name="x">The uint to be bitshifted</param>
		/// <param name="n">The number of bits to shift over</param>
		/// <returns></returns>
		private static uint RotateLeft(uint x, byte n)
		{
			return ((x << n) | (x >> (32 - n)));
		}

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
		/// <summary>
		/// For getting the number of set AreaFlags contained in the flag it is called on.
		/// </summary>
		/// <param name="baseFlag">I wonder how many flags are in this. Let's find out!</param>
		/// <returns>0 to 16 pads (depending on how many are in baseFlag)</returns>
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
		/// <summary>
		/// A chainable function for adding flags together.
		/// Functionally identical to (baseFlag = baseFlag | added)
		/// </summary>
		/// <param name="baseFlag">The flag we want to add more flags to</param>
		/// <param name="added">The flags to add to the baseFlag</param>
		/// <returns>The resulted added flag.</returns>
		public static AreaFlag AddFlag(this AreaFlag baseFlag, AreaFlag added)
		{
			baseFlag = baseFlag | added;
			return baseFlag;
		}

		/// <summary>
		/// Check if the checked flag is set inside of baseFlag.
		/// </summary>
		/// <param name="baseFlag">The flag that might contain checkFlag</param>
		/// <param name="checkFlag">The flag(s) that we want to look for, can accept complex flags</param>
		/// <returns>Whether or not the base flag has ALL of the flags in checkFlag</returns>
		public static bool HasFlag(this AreaFlag baseFlag, AreaFlag checkFlag)
		{
			return HasFlag(baseFlag, (int)checkFlag);
		}

		/// <summary>
		/// An overload to use numerical values to see if we have the the requested flag(s)
		/// </summary>
		/// <param name="baseFlag">The flag that might contain the int equivalent (flag)</param>
		/// <param name="flag">A value between 0 and 16711936</param>
		/// <returns>Whether or not the base flag has ALL of the flags in flag (converted to an AreaFlag)</returns>
		public static bool HasFlag(this AreaFlag baseFlag, int flag)
		{
			if (((int)baseFlag & (flag)) == flag)
			{
				return true;
			}
			return false;
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
		/// <returns>Returns an array of single AreaFlags. Will not include None, Boths or All AreaFlag values.</returns>
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

		//public static AreaFlag RandomSubset(this AreaFlag complexFlag, uint count = 1)
		//{
		//	if (count == 0)
		//		return AreaFlag.None;

		//	if (complexFlag.NumberOfAreas() <= count)
		//		return complexFlag;
		//	System.Random
		//	AreaFlag temp = complexFlag;

		//	for (int i = 0; i < count; i++)
		//	{
		//		complexFlag.
		//	}

		//	return AreaFlag.None;
		//}
	}
}