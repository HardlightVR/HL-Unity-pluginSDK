using System.Collections.Generic;
using NullSpace.API.Enums;

namespace NullSpace.SDK.Editor
{
    public abstract class HapticArgs
    {
        private int _hashCode;
        private readonly string _name;
        protected readonly List<object> _args;

        public string Name
        {
            get { return _name; }
        }

        protected HapticArgs(string name)
        {
            _args = new List<object>();
            _name = name;
            _args.Add(name);
        }

        protected void GenerateCombinedHash()
        {
            unchecked
            {
                int hash = 17;
                foreach (var obj in _args)
                {
                    hash = hash * 31 + obj.GetHashCode();
                }
                _hashCode = hash;
            }
        }

        public int GetCombinedHash()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    public class PatternArgs : HapticArgs
    {
        private readonly Side _side;

        public Side Side
        {
            get { return _side; }
        }
        public PatternArgs(string name, Side side) : base(name)
        {
            _side = side;
            _args.Add(side);
            GenerateCombinedHash();
        }
    }

    public class ExperienceArgs : HapticArgs
    {
        private readonly Side _side;

        public Side Side
        {
            get { return _side; }
        }
        public ExperienceArgs(string name, Side side) : base(name)
        {
            _side = side;
            _args.Add(side);
            GenerateCombinedHash();
        }
    }

    public class SequenceArgs : HapticArgs
    {
        private readonly Location _location;

        public Location Location
        {
            get { return _location; }
        }
        public SequenceArgs(string name, Location location) : base(name)
        {
            _location = location;
            _args.Add(location);
            GenerateCombinedHash();
        }
    }

}