namespace GalacticWasteManagement
{
    public interface IInput
    {
        string Name { get; }

        void TrySet<T>(Param<T> param);
    }
}