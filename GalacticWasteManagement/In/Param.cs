namespace GalacticWasteManagement
{
    public class Param<T>
    {
        internal InputParam<T> inputParam;
        private readonly T defaultValue;
        internal readonly bool optional;
        private readonly Input input;

        public InputValue<T> Value { get; internal set; }

        public Param(InputParam<T> inputParam, T defaultValue, bool optional, Input input)
        {
            this.inputParam = inputParam;
            this.defaultValue = defaultValue;
            this.optional = optional;
            this.input = input;
        }

        public void SetValue(T value)
        {
            Value = InputValue<T>.Of(value);
        }

        public T Get()
        {
            if(Value == null)
            {
                input.Set(this);
            }

            return Value.value;
        }
    }
}