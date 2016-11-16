/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using System.Linq;
using NullSpace.API.Enums;
using NullSpace.API.Logger;
using NullSpace.SDK.Editor.Parser;
using NullSpace.SDK.Haptics;
namespace NullSpace.SDK.Editor
{

	public class HapticRef
	{
		public int hashCode;
		public HapticRef(int hashcode)
		{
			this.hashCode = hashcode;
		}

	}
	public class PatternRef : HapticRef
	{
		public PatternRef(int hashcode) : base(hashcode)
		{

		}
	}

	public class SequenceRef : HapticRef
	{
		public SequenceRef(int hashcode) : base(hashcode)
		{

		}
	}

	public class ExperienceRef : HapticRef
	{
		public ExperienceRef(int hashcode) : base(hashcode)
		{

		}
	}
	/// <summary>
	/// The DependencyResolver loads haptic files, allowing for programatic playing of files and dynamic side-switching
	/// </summary>
	public class DependencyResolver
	{
		private Dictionary<string, List<SequenceItem>> sequences;
		private Dictionary<string, List<Moment>> experiences;
		private Dictionary<string, List<Frame>> patterns;
		private Dictionary<int, List<HapticEffect>> resolvedSequences;
		private Dictionary<int, List<HapticFrame>> resolvedPatterns;
		private Dictionary<int, List<HapticSample>> resolvedExperiences;
		private HapticFileParser parser;
		private Object thisLock = new Object();

		/// <summary>
		/// Create a new DependencyResolver
		/// </summary>
		/// <param name="parser">The file parser</param>
		public DependencyResolver(HapticFileParser parser)
		{
			this.parser = parser;
			this.sequences = new Dictionary<string, List<SequenceItem>>();
			this.patterns = new Dictionary<string, List<Frame>>();
			this.experiences = new Dictionary<string, List<Moment>>();
			this.resolvedSequences = new Dictionary<int, List<HapticEffect>>();
			this.resolvedExperiences = new Dictionary<int, List<HapticSample>>();
			this.resolvedPatterns = new Dictionary<int, List<HapticFrame>>();
		}








		public void LoadAsync(KeyValuePair<string, HapticFileType> fileInfo)
		{
			if (HapticFileParser.HasExtension(fileInfo.Key))
			{
				if (HapticFileParser.DetermineFileType(fileInfo.Key) != fileInfo.Value)
				{
					Log.Warning("The file extension you are using ({0}) does not match the method type you called ({1}). Aborting.");
					return;
				}
				else
				{
					this.Load(fileInfo.Key);
				}
			}
			else
			{
				//They did not provide an extension, and we are lazy loading!
				//We have to find out what it could be

				string[] validNames = HapticFileParser.GetValidFileNames(fileInfo.Key, fileInfo.Value);
				foreach (var name in validNames)
				{
					try
					{
						this.Load(name);
						return;
					}
					catch (System.IO.FileNotFoundException)
					{
						continue;
					}
					catch (Exception e)
					{
						Log.Warning("There was a problem parsing the haptic file {0}: {1}", name, e.Message);
						return;
						//abort
					}
				}
				Log.Warning("Tried to look for file {0} in files {1} and failed.", fileInfo.Key, string.Join(", ", validNames));
				throw new System.IO.FileNotFoundException();

			}
		}

		public void Load(string path)
		{
			lock (thisLock)
			{
				HapticFileType fileType = HapticFileParser.DetermineFileType(path);


				switch (fileType)
				{
					case HapticFileType.Experience:
						this.LoadExperience(path);
						break;
					case HapticFileType.Pattern:
						this.LoadPattern(path);
						break;
					case HapticFileType.Sequence:
						this.LoadSequence(path);
						break;
					case HapticFileType.Unrecognized:

					default:
						Log.Error("Could not load file {0}: file type is unrecognized (expected .pattern, .sequence, or .experience)", path);
						throw new System.IO.FileNotFoundException();

				}
			}
		}

		/// <summary>
		/// Loads a haptic sequence file, storing it for future use
		/// </summary>
		/// <param name="path">The name of the file</param>
		private void LoadSequence(string path)
		{
			if (this.sequences.ContainsKey(stripPath(path)))
			{
				return;
			}

			List<SequenceItem> sequence = parser.ParseSequence(path);
			this.sequences[getFileName(path)] = sequence;
		}
		public bool Loaded(string name, HapticFileType ftype)
		{
			lock (thisLock)
			{
				switch (ftype)
				{
					case HapticFileType.Pattern:
						return this.patterns.ContainsKey(name);
					case HapticFileType.Experience:
						return this.experiences.ContainsKey(name);
					case HapticFileType.Sequence:
						return this.sequences.ContainsKey(name);
					default:
						Log.Warning("Couldn't check if unrecognized file {0} is loaded", name);
						return false;
				}
			}
		}
		public bool Loaded(string name)
		{
			lock (thisLock)
			{
				switch (HapticFileParser.DetermineFileType(name))
				{
					case HapticFileType.Pattern:
						return this.patterns.ContainsKey(name);
					case HapticFileType.Experience:
						return this.experiences.ContainsKey(name);
					case HapticFileType.Sequence:
						return this.sequences.ContainsKey(name);
					default:
						Log.Warning("File {0} not recognized", name);
						return false;
				}

			}
		}

		private string stripPath(string path)
		{
			string[] res = path.Split('.');
			return res[0];
		}
		/// <summary> 
		/// Loads a haptic pattern file, storing it for future use
		/// </summary>
		/// <param name="path">The name of the file</param>
		private void LoadPattern(string path)
		{
			if (this.patterns.ContainsKey(stripPath(path)))
			{
				return;
			}

			List<Frame> pattern = parser.ParsePattern(path);
			this.patterns[getFileName(path)] = pattern;
			foreach (var frame in pattern)
			{
				foreach (var sequence in frame.frame)
				{
					this.LoadSequence(sequence.sequence);
				}
			}

		}
		private static string getFileName(string path)
		{
			return path.Split('.')[0];
		}

		public static string makeFullName(string fname, string ext)
		{
			return string.Format("{0}.{1}", fname, ext);
		}
		/// <summary>
		/// Loads a haptic experience file
		/// </summary>
		/// <param name="path">The name of the file</param>
		private void LoadExperience(string path)
		{
			if (this.experiences.ContainsKey(stripPath(path)))
			{
				return;
			}

			List<ISample> unprocessedSamples = parser.ParseExperience(path);

			//A moment will store a sample and a time offset, which samples lack on their own
			List<Moment> processedSamples = new List<Moment>(unprocessedSamples.Count);

			foreach (var sample in unprocessedSamples)
			{
				//We need a name for the pattern.
				//In some files, this might be specified. In others, it could be anonymously
				//defined inline.

				string patternName;
				//If the sample is a placeholder, we can grab the pattern name and load it
				if (sample.GetType() == typeof(SamplePlaceholder))
				{
					patternName = ((SamplePlaceholder)sample).pattern;
					//loads the given pattern into our patterns cache
					this.LoadPattern(patternName);
				}
				else
				{
					// else, the sample came with an inline list of frames -- load that, and generate a pattern name
					patternName = this.LoadInlinePattern(((Sample)sample).pattern);
				}

				patternName = HapticFileParser.GetNameWithoutExtension(patternName);

				//Either way, we now have the ingredients for a new Moment. We fetch the pattern from our
				//cache, grab the sample time & side, and add the moment to processedSamples.
				processedSamples.Add(new Moment(patternName, sample.time, this.patterns[stripPath(patternName)], sample.SideEnum));

				//Also, at this point we should deal with samples containing the "repeat" key, which allows a 
				//sample to be repeated without copy & paste.
				if (sample.repeat > 0)
				{
					//this offset is kind of arbitary now, but serves as a buffer before the repetition happens.
					//getLatestTime finds the pattern with the farthest out time offset
					float offset = 0.1f + this.getLatestTime(patternName);
					//since we already placed one sample in, we start with repetition = 1
					for (int repetition = 1; repetition < sample.repeat; repetition++)
					{
						processedSamples.Add(new Moment(
							patternName,
							//Original sample time + (total time offset * repetition)
							sample.time + offset * repetition,
							//pattern from cache
							this.patterns[stripPath(patternName)],
							//which side 
							sample.SideEnum
							//name of the pattern for future lookups
							));
					}
				}
				else
				{
					//else they want a oneshot -- which we added above already
				}
			}

			//cache this experience
			this.experiences[getFileName(path)] = processedSamples;

		}

		private float getLatestTime(string patternName)
		{
			var pattern = this.patterns[patternName];
			return pattern.Max(t => t.time);
		}

		private string LoadInlinePattern(List<Frame> pattern)
		{
			foreach (var frame in pattern)
			{
				foreach (var sequence in frame.frame)
				{
					this.LoadSequence(sequence.sequence);
				}
			}
			string guid = Guid.NewGuid().ToString();
			this.patterns[guid] = pattern;
			return guid;
		}



		/// <summary>
		/// Get HapticSample data from an ExperienceRef
		/// </summary>
		/// <param name="experienceRef">A hashcode reference to an experience, obtained by ResolveExperience</param>
		/// <returns></returns>
		public List<HapticSample> GetData(ExperienceRef experienceRef)
		{
			return this.resolvedExperiences[experienceRef.hashCode];
		}

		/// <summary>
		/// Get HapticFrame data from a PatternRef
		/// </summary>
		/// <param name="patternRef">A hashcode reference to a pattern, obtained by ResolvePattern</param>
		/// <returns></returns>
		public List<HapticFrame> GetData(PatternRef patternRef)
		{
			return this.resolvedPatterns[patternRef.hashCode];
		}


		/// <summary>
		/// Get HapticEffect data from a SequenceRef
		/// </summary>
		/// <param name="sequenceRef">A hashcode reference to a sequence, obtained by ResolveSequence</param>
		/// <returns></returns>
		public List<HapticEffect> GetData(SequenceRef sequenceRef)
		{
			return this.resolvedSequences[sequenceRef.hashCode];
		}

		/// <summary>
		/// Resolves a sequence file
		/// </summary>
		/// <param name="name">Name of the sequence</param>
		/// <param name="location">Location to play the sequence</param>
		/// <returns>A reference to the sequence</returns>
		public SequenceRef ResolveSequence(string name, Location location)
		{
			//Early out if the sequence is not loaded
			if (!this.sequences.ContainsKey(name))
			{
				Log.Warning("Couldn't find a sequence named {0}", name);
				return null;
			}

			//if we already resolved something with the same name and location,
			//don't bother doing it again. It's already cached.
			if (this.resolvedSequences.ContainsKey(combineHash(name, location)))
			{
				return new SequenceRef(combineHash(name, location));
			}


			List<SequenceItem> inputItems = this.sequences[name];
			List<HapticEffect> outputEffects = new List<HapticEffect>(inputItems.Count);

			foreach (var seqItem in inputItems)
			{
				try
				{
					outputEffects.Add(transformSequenceItemToEffect(seqItem, location));
				}
				catch
				{
					//if we failed to parse the enum (see transformSequenceItemToEffect), 
					//give a warning and continue to the next item
					//TODO: should this fail instead of continue?
					Log.Warning("Could not find waveform {0} in sequence {1}", seqItem.atom.waveform, name);
					continue;
				}
			}

			//cache this sequence for later possible re-use
			this.resolvedSequences[combineHash(name, location)] = outputEffects;
			return new SequenceRef(combineHash(name, location));
		}

		/// <summary>
		/// Helper method for ResolveSequence
		/// </summary>
		/// <param name="item">An input SequenceItem</param>
		/// <param name="location">An input Location</param>
		/// <returns>The Effect corresponding to the given sequence at location</returns>
		private HapticEffect transformSequenceItemToEffect(SequenceItem item, Location location)
		{
			//have no TryParse, so could throw
			Effect effect = (Effect)Enum.Parse(typeof(Effect), item.atom.waveform);
			return new HapticEffect(effect, location, item.atom.duration, item.time, 1);

		}
		/// <summary>
		/// Resolves a pattern file
		/// </summary>
		/// <param name="name">The name of the pattern</param>
		/// <param name="programmaticSide">The Side on which to play the pattern. </param>
		/// <returns>A reference to the pattern</returns>
		public PatternRef ResolvePattern(string name, Side programmaticSide = Side.Mirror)
		{
			//Early out if the pattern is not loaded yet
			if (!this.patterns.ContainsKey(name))
			{
				Log.Warning("Couldn't find a pattern named {0}", name);
				return null;
			}

			//if already cached in patterns dictionary, return it
			if (this.resolvedPatterns.ContainsKey(combineHash(name, programmaticSide)))
			{
				return new PatternRef(combineHash(name, programmaticSide));
			}

			List<HapticFrame> frames = new List<HapticFrame>();
			foreach (var frame in this.patterns[name])
			{
				frames.Add(transformFrameToHapticFrame(frame, programmaticSide));
			}
			this.resolvedPatterns[combineHash(name, programmaticSide)] = frames;
			return new PatternRef(combineHash(name, programmaticSide));
		}

		//transform a frame into a resolved haptic frame
		//given: Frame frame, Side progSide


		//foreach JEffect seq in frame.frame
		//inputSide = fromEnum(seq.side)
		//

		/// <summary>
		/// Helper method for ResolvePattern
		/// </summary>
		/// <param name="frame">An input Frame</param>
		/// <param name="programmaticSide">An input Side</param>
		/// <returns>The HapticFrame constructed by transforming the input Frame by the given Side</returns>
		private HapticFrame transformFrameToHapticFrame(Frame frame, Side programmaticSide)
		{
			List<HapticSequence> sequences = new List<HapticSequence>();
			foreach (var inputSeq in frame.frame)
			{
				Side actualSide = computeSidePrecedence(HapticFileParser.SideFromEnum(inputSeq.side), programmaticSide);


				//names are stored in the hashtables without extensions
				string name = HapticFileParser.GetNameWithoutExtension(inputSeq.sequence);
				switch (actualSide)
				{
					case Side.Inherit:
						//should not happen!
						Log.Error("Logic is broken, please call NullSpace VR");
						break;
					case Side.Mirror:
						var left = this.ResolveSequence(name, computeLocationSide(inputSeq.location, Side.Left));
						sequences.Add(new HapticSequence(GetData(left)));
						var right = this.ResolveSequence(name, computeLocationSide(inputSeq.location, Side.Right));
						sequences.Add(new HapticSequence(GetData(right)));
						break;
					default:
						var specific = this.ResolveSequence(name, computeLocationSide(inputSeq.location, actualSide));
						sequences.Add(new HapticSequence(GetData(specific)));
						break;
				}
			}
			return new HapticFrame(frame.time, sequences);
		}

		/// <summary>
		/// Helper method to build a list of HapticSequences. 
		/// </summary>
		/// <param name="sequences">The list of HapticSequences to insert into</param>
		/// <param name="sequenceName">Name of the sequence</param>
		/// <param name="loc">Which location to play the sequence on</param>
		private void addSequence(List<HapticSequence> sequences, string sequenceName, Location loc)
		{
			//Grab a reference to the sequence
			var href = this.ResolveSequence(sequenceName, loc);
			//Construct a new HapticSequence, which is basically a wrapper, around the actual data
			sequences.Add(new HapticSequence(GetData(href)));
		}


		/// <summary>
		/// This method finds the correct side to use, given a Side specified in a file and one specified programmatically. 
		/// This is necessary because we want to be able to substitute in a programmatic side if the programmer uses one,
		/// but not overwrite specific sides written in the file.
		/// </summary>
		/// <param name="inputSide">The Side given in a file</param>
		/// <param name="programmaticSide">The Side given by the programmer at runtime</param>
		/// <returns>The side that takes precedence</returns>
		private Side computeSidePrecedence(Side inputSide, Side programmaticSide)
		{
			switch (inputSide)
			{
				case Side.NotSpecified:
					return programmaticSide;
				//The interesting case is when the file-defined side is inherit.
				case Side.Inherit:
					return programmaticSide == Side.Inherit ? Side.Mirror : programmaticSide;
				//If the input side is specified, then return it. 
				default:
					return inputSide;
			}

		}


		/// <summary>
		/// Resolves an experience file
		/// </summary>
		/// <param name="name">Name of the experience</param>
		/// <param name="side">Optional side on which to play the experience</param>
		/// <returns>A reference to the experience</returns>
		public ExperienceRef ResolveExperience(string name, Side side = Side.Mirror)
		{
			//Early out if the experience has not been loaded yet
			if (!this.experiences.ContainsKey(name))
			{
				Log.Warning("Couldn't find an experience named {0}", name);
				return null;
			}

			//If the experience with the desired side was already resolved, return the 
			//cached copy
			if (this.resolvedExperiences.ContainsKey(combineHash(name, side)))
			{
				return new ExperienceRef(combineHash(name, side));
			}

			//Else, we need to do some computation.
			//First, grab the unprocessed experience data. 
			List<Moment> inputMoments = this.experiences[name];
			//Setup the output list, which is what we are computing. 
			List<HapticSample> outputSamples = new List<HapticSample>(inputMoments.Count);

			foreach (var moment in inputMoments)
			{
				outputSamples.Add(transformMomentToHapticSample(moment, side));
			}

			//Store in the cache
			this.resolvedExperiences[combineHash(name, side)] = outputSamples;
			return new ExperienceRef(combineHash(name, side));
		}

		/// <summary>
		/// Helper function for ResolveExperience
		/// </summary>
		/// <param name="moment">An input Moment</param>
		/// <param name="side">A given Side</param>
		/// <returns>The result of transforming the Moment into a HapticSample</returns>
		private HapticSample transformMomentToHapticSample(Moment moment, Side side)
		{
			PatternRef pref;
			//If the developer specified a side in the file,
			if (moment.side != Side.NotSpecified && moment.side != Side.Inherit)
			{
				//then resolve the pattern using that side.
				pref = this.ResolvePattern(moment.name, moment.side);
			}
			else
			{
				//else, use the programmatic side given at runtime 
				pref = this.ResolvePattern(moment.name, side);
			}

			//Grab the actual data from the cache
			List<HapticFrame> frames = GetData(pref);

			//TODO: Priority here is 1. Change to something higher? Programmer specified?
			return new HapticSample(moment.time, frames, 1);
		}


		/// <summary>
		/// Given a string representing a location on the suit, and a desired side, compute the specific location.
		/// For example, given "shoulder" and Side.Left, returns Location.Shoulder_Left
		/// </summary>
		/// <param name="loc">String representing a possible suit location</param>
		/// <param name="side">Desired side</param>
		/// <returns>The specific location obtained from the given string and Side</returns>
		private Location computeLocationSide(string loc, Side side)
		{
			switch (loc)
			{
				case "shoulder":
					return side == Side.Left ? Location.Shoulder_Left : Location.Shoulder_Right;
				case "upper_back":
					return side == Side.Left ? Location.Upper_Back_Left : Location.Upper_Back_Right;
				case "lower_ab":
					return side == Side.Left ? Location.Lower_Ab_Left : Location.Lower_Ab_Right;
				case "mid_ab":
					return side == Side.Left ? Location.Mid_Ab_Left : Location.Mid_Ab_Right;
				case "upper_ab":
					return side == Side.Left ? Location.Upper_Ab_Left : Location.Upper_Ab_Right;
				case "chest":
					return side == Side.Left ? Location.Chest_Left : Location.Chest_Right;
				case "upper_arm":
					return side == Side.Left ? Location.Upper_Arm_Left : Location.Upper_Arm_Right;
				case "forearm":
					return side == Side.Left ? Location.Forearm_Left : Location.Forearm_Right;
				default:
					return Location.Error;
			}
		}


		private int combineHash(object a, object b)
		{
			int hash = 17;
			hash = hash * 31 + a.GetHashCode();
			hash = hash * 31 + b.GetHashCode();
			return hash;
		}

	}
}
