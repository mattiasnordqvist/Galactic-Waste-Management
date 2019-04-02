namespace GalacticWasteManagement
{
    public class InputParam<T>
    {
        public string Name { get; }
        public string Description { get; }

        public InputParam(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}