namespace GalacticWasteManagement
{
    public class InputValue<T>
    {
        private T _value;

        public T Value
        {
            get{
                if (HasValue) { return _value; }

                else { throw new System.Exception("Input has no value"); }
            }
            private set => _value = value;
        }

        public InputValue()
        {
            HasValue = false;
        }

        public InputValue(T value)
        {
            Value = value;
            HasValue = true;
        }

        public static InputValue<T> Null
        {
            get { return new InputValue<T>(); }
        }

        public bool HasValue { get; private set; }

        public static InputValue<T> Of(T value)
        {
            return new InputValue<T>(value);
        }
    }
}