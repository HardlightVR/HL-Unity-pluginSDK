using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullSpace.SDK.Generators
{
	/// <summary>
	/// Generates a pattern which plays an effect on a random location.
	/// </summary>
	public class RandomAreaGenerator : IHapticGenerator<CodeSequence, CodePattern>
	{
		private System.Random _random;
		private IList<AreaFlag> _areas;
		private AreaFlag _currentArea;
		public RandomAreaGenerator()
		{
			//Shortcut to grab only the 'single' area flags
			var flags = (AreaFlag[]) Enum.GetValues(typeof(AreaFlag));
			_areas = flags.Where(flag => 
				flag.ToString().Contains("_Left") || 
				flag.ToString().Contains("_Right")).ToList();

			_random = new System.Random();
			
			//Pick the first random location
			Next();
		}

		/// <summary>
		/// Pick a new random area
		/// </summary>
		/// <returns>A reference to this</returns>
		public RandomAreaGenerator Next()
		{

			int which = _random.Next(0, _areas.Count);
			_currentArea = _areas[which];
			return this;
		}

		/// <summary>
		/// Generate the effect, given a sequence to play on the area
		/// </summary>
		/// <param name="seq">The sequence to play on the area</param>
		/// <returns>A pattern which can be played</returns>
		public CodePattern Generate(CodeSequence seq)
		{
			CodePattern p = new CodePattern();
			p.AddChild(0f, _currentArea, seq);
			return p;
		}

		/// <summary>
		/// Convenience method to generate the effect while picking a new area
		/// </summary>
		/// <param name="seq">The sequence to play on the area</param>
		/// <returns>A pattern which can be played</returns>
		public CodePattern GenerateNext(CodeSequence seq)
		{
			return Next().Generate(seq);
		}

	}
}
