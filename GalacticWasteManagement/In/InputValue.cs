namespace GalacticWasteManagement
{
    public class InputValue<T>
    {
        internal T value;

        public InputValue()
        {
        }

        public InputValue(T value)
        {
            this.value = value;
        }

        public static InputValue<T> Null
        {
            get { return new InputValue<T>(); }
        }
        public static InputValue<T> Of(T value) 
        {
            return new InputValue<T>(value );
        }
    }
}