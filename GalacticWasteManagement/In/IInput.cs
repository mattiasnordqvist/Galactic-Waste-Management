namespace GalacticWasteManagement
{
    public interface IInput
    {
        void TrySet<T>(Param<T> param);
    }
}