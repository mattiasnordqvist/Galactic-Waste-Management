using System;
using System.IO;

namespace GalacticWasteManagement
{
    public class InputFile : InputParam<string>
    {
        public InputFile(string name, string description, bool mustExist)
            : base(name, description, (file) =>
            {
                if (mustExist && !File.Exists(file))
                {
                    throw new Exception($"File {file} does not exist");
                }
                return file;
            })
        {
        }
    }

    public class InputBool : InputParam<bool>
    {
        public InputBool(string name, string description)
            : base(name, description, x =>
            {
                return bool.Parse(x);
            })
        {
        }
    }

    public class InputString : InputParam<string>
    {
        public InputString(string name, string description)
            : base(name, description, x => x)
        {
        }
    }

    public class InputParam<T>
    {
        public Func<string, T> Parse;
        public string Name { get; }
        public string Description { get; }

        public InputParam(string name, string description, Func<string, T> parse)
        {
            Name = name;
            Description = description;
            Parse = parse;
        }
    }
}