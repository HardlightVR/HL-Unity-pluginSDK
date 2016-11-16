/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System.Collections.Generic;
using LitJson;
using NullSpace.API.Enums;
using NullSpace.API.Logger;
namespace NullSpace.SDK.Editor.Parser
{



    /// <summary>
    /// The lowest conceptual level of haptics
    /// </summary>
    public class Atom
    {
        /// <summary>
        /// A wave form
        /// </summary>
		public string waveform;
        /// <summary>
        /// A duration in seconds
        /// </summary>
		public float duration;
        public Atom()
        {
        }
    }

    /// <summary>
    /// A SequenceItem is one step above an Atom, but includes a time offset
    /// </summary>
	public class SequenceItem
    {
        /// <summary>
        /// Time offset in seconds
        /// </summary>
		public float time;
        /// <summary>
        /// Associated Atom
        /// </summary>
		public Atom atom;
        public SequenceItem()
        {
        }
    }

    /// <summary>
    /// The initial JSON representation of an effect
    /// </summary>
	public class JEffect
    {
        /// <summary>
        /// The associated sequence file
        /// </summary>
		public string sequence;
        /// <summary>
        /// Location on which to play the sequence
        /// </summary>
		public string location;
        /// <summary>
        /// Side on which to play the sequence
        /// </summary>
		public string side;
        /// <summary>
        /// The list of SequenceItems associated with the name
        /// </summary>
		public List<SequenceItem> sequenceList;

        public JEffect(string seq, string loc, string side)
        {
            this.sequence = seq;
            this.location = loc;
            this.side = side;
        }
        public JEffect()
        {
        }
    }

    /// <summary>
    /// A frame contains multiple JEffects, and a time offset
    /// </summary>
	public class Frame
    {
        /// <summary>
        /// The time offset in seconds
        /// </summary>
		public float time;
        /// <summary>
        /// The list of JEffects constituting the frame
        /// </summary>
		public List<JEffect> frame;
        public Frame(float time, List<JEffect> frame)
        {
            this.time = time;
            this.frame = frame;
        }
        public Frame()
        {
        }
    }

    /// <summary>
    /// A moment is simply a pattern scheduled to play at a certain time, and on a certain side of the suit
    /// </summary>
    public class Moment
    {
        public string name;
        public List<Frame> pattern;
        public float time;
        public Side side;
        public Moment(string name, float t, List<Frame> p, Side side = Side.Inherit)
        {
            this.pattern = p;
            this.time = t;
            this.side = side;
            this.name = name;
        }
    }

    /// <summary>
    /// Interface to handle JSON parsing of samples. A sample might have a placeholder for pattern name, 
    /// or an inline definition. In either case, the sample will have a time, Side, and repeat variable.
    /// </summary>
	public interface ISample
    {
        float time
        {
            get; set;
        }


        Side SideEnum
        {
            get; set;
        }

        int repeat
        {
            get; set;
        }
    }

    public class Sample : ISample
    {

        /// <summary>
        /// How many times to repeat the pattern
        /// </summary>
        public int repeat { get; set; }

        /// <summary>
        /// Time offset in seconds
        /// </summary>
        public float time { get; set; }
        /// <summary>
        /// List of frames representing an inline pattern
        /// </summary>
        public List<Frame> pattern;
        /// <summary>
        /// Indicate the Side of the suit to play the pattern on
        /// </summary>
        public Side SideEnum
        {
            get; set;
        }

        /// <summary>
        /// Construct a new sample
        /// </summary>
        /// <param name="time">Time offset in seconds</param>
        /// <param name="frames">List of Frames that constitute the pattern</param>
        public Sample(float time, List<Frame> frames)
        {
            this.time = time;
            this.pattern = frames;
            this.SideEnum = Side.NotSpecified;
        }
        public Sample()
        {
        }
    }

    /// <summary>
    /// Represents a sample with a string as the pattern parameter. This will later be resolved by loading the correct
    /// pattern.
    /// </summary>
	public class SamplePlaceholder : ISample
    {
        /// <summary>
        /// Pattern file name
        /// </summary>
		public string pattern;

        /// <summary>
        /// Time offset in seconds
        /// </summary>
        public float time { get; set; }
        /// <summary>
        /// How many times to repeat the pattern
        /// </summary>
        public int repeat { get; set; }

        /// <summary>
        /// Indicate the Side of the suit to play the pattern on
        /// </summary>
        public Side SideEnum
        {
            get; set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SamplePlaceholder()
        {
        }
    }

    /// <summary>
    /// HapticFileParser is responsible for loading haptic files and generating lists of ISamples.
    /// </summary>
	public class HapticFileParser
    {
        /// <summary>
        /// The base path where the parser will search for files
        /// </summary>
		public string BasePath = "";


        public HapticFileParser()
        {
        }
        public static HapticFileType DetermineFileType(string path)
        {
            //could use Path.GetExtension(..) but then would limit to actual files
            string[] extensions = path.Split('.');
            if (extensions.Length == 1)
            {
                return HapticFileType.Unrecognized;
            }

            string last = extensions[extensions.Length - 1];
            switch (last)
            {
                case "pattern":
                case "pat":
                    return HapticFileType.Pattern;
                case "experience":
                case "exp":
                    return HapticFileType.Experience;
                case "sequence":
                case "seq":
                    return HapticFileType.Sequence;
                default:
                    return HapticFileType.Unrecognized;
            }
        }
        public static string GetNameWithoutExtension(string path)
        {
            string[] parts = path.Split('.');
            return parts[0];
        }
        public static bool HasExtension(string path)
        {
            return path.Split('.').Length > 1;
        }
        public static string GetExtension(string path)
        {
            string[] parts = path.Split('.');
            return parts[parts.Length - 1];
        }

        public static string[] GetValidFileNames(string name, HapticFileType ftype)
        {
            string[] seqExtensions = { "seq", "sequence" };
            string[] patExtensions = { "pat", "pattern" };
            string[] expExtensions = { "exp", "experience" };

            string fileTemplate = "{0}.{1}";
            switch (ftype)
            {
                case HapticFileType.Experience:
                    return permute(name, expExtensions, fileTemplate);
                case HapticFileType.Pattern:
                    return permute(name, patExtensions, fileTemplate);
                case HapticFileType.Sequence:
                    return permute(name, seqExtensions, fileTemplate);
                default:
                    Log.Error("Tried to fetch valid filenames for an unrecognized type.. does not compute.");
                    return new string[] { };
            }   
        }
        private static string[] permute(string a, string[] b, string template)
        {
            string[] result = new string[b.Length];
            for (int i = 0; i < b.Length; i++)
            {
                result[i] = string.Format(template, a, b[i]);
            }
            return result;
        }
        private KeyValuePair<HapticFileType, string> loadJson(string path)
        {
            try
            {
                var fileResult = HapticResourceLoader.Load(getPath(path));
                if (fileResult.Key == HapticFileType.Unrecognized)
                {
                    Log.Error("Could not load the haptics file at path {0}: Unrecognized file type.\nUse .sequence, .pattern, or .experience", path);
                    throw new System.IO.FileLoadException();
                }
                return fileResult;
            }
            catch (System.IO.FileNotFoundException e)
            {
                Log.Error("Could not find the haptics file at path {0}: {1}", path, e.Message);
                throw;
            }
        }


        /// <summary>
        /// Parse an experience file
        /// </summary>
        /// <param name="path">File name</param>
        /// <returns>The list of ISamples constituting the experience</returns>
        public List<ISample> ParseExperience(string path)
        {
            string json = System.IO.File.ReadAllText(getPath(path));
            JsonData data = JsonMapper.ToObject(json);

            List<ISample> samples = new List<ISample>(data["experience"].Count);
            for (int i = 0; i < data["experience"].Count; i++) 
            {
                string expData = data["experience"][i].ToJson(); ;
                JsonData tempData = JsonMapper.ToObject(expData);

                if (tempData["pattern"].GetJsonType() == JsonType.Array)
                {
                    Sample s = JsonMapper.ToObject<Sample>(new JsonReader(expData));
                    if (tempData.Keys.Contains("side"))
                    {
                        s.SideEnum = SideFromEnum(tempData["side"].GetString());
                    }
                    else
                    {
                        s.SideEnum = Side.NotSpecified;
                    }
                    samples.Add(s);
                }
                else if (tempData["pattern"].GetJsonType() == JsonType.String)
                {
                    SamplePlaceholder s = JsonMapper.ToObject<SamplePlaceholder>(new JsonReader(expData));
                    if (tempData.Keys.Contains("side"))
                    {
                        s.SideEnum = SideFromEnum(tempData["side"].GetString());
                    }
                    else
                    {
                        s.SideEnum = Side.NotSpecified;
                    }
                    samples.Add(s);
                }


            }

            return samples;


        }

        /// <summary>
        /// Convert a string representation of the Side enum into a Side, returning Side.NotSpecified on failure
        /// </summary>
        /// <param name="side">The side</param>
        /// <returns>The Side</returns>
        public static Side SideFromEnum(string side)
        {
            switch (side)
            {
                case "left":
                    return Side.Left;
                case "right":
                    return Side.Right;
                case "mirror":
                    return Side.Mirror;
                case "inherit":
                    return Side.Inherit;
                default:
                    return Side.NotSpecified;
            }
        }

        /// <summary>
        /// Parse a sequence file
        /// </summary>
        /// <param name="path">File name</param>
        /// <returns>The list of SequenceItems constituting the sequence</returns>
        public List<SequenceItem> ParseSequence(string path)
        {
            string json = System.IO.File.ReadAllText(getPath(path));
            JsonData data = JsonMapper.ToObject(json);
           
            List<SequenceItem> items = new List<SequenceItem>(data["sequence"].Count);
            for (int i = 0; i < data["sequence"].Count; i++) { 
                SequenceItem s = JsonMapper.ToObject<SequenceItem>(new JsonReader(data["sequence"][i].ToJson()));
                items.Add(s);
            }
            return items;
        }

        /// <summary>
        /// Parse a pattern file
        /// </summary>
        /// <param name="path">File name</param>
        /// <returns>The list of Frames constituting the pattern</returns>
        public List<Frame> ParsePattern(string path)
        {
            string json = System.IO.File.ReadAllText(getPath(path));

           
            JsonData data = JsonMapper.ToObject(json);


            List<Frame> items = new List<Frame>(data["pattern"].Count);
            for (int i = 0; i < data["pattern"].Count; i++)
            {
                Frame f = JsonMapper.ToObject<Frame>(new JsonReader(data["pattern"][i].ToJson()));
                items.Add(f);
            }
            return items;
           

        }
        private string getPath(string path)
        {
            return System.IO.Path.Combine(BasePath, path);
          
        }
    }

}

