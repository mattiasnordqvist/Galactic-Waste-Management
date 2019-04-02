namespace GalacticWasteManagement
{

    public abstract class Input
    {
        public abstract Param<T> Optional<T>(InputParam<T> inputParam, T defaultValue);
        public abstract Param<T> Required<T>(InputParam<T> inputParam);
        public abstract void Set<T>(Param<T> param);
    }
}