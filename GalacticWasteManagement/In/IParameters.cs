using System.Collections.Generic;

namespace GalacticWasteManagement
{
    public interface IParameters
    {
        Param<T> Optional<T>(InputParam<T> inputParam, T defaultValue);
        Param<T> Required<T>(InputParam<T> inputParam);
        void Supply(Dictionary<string, object> parameters);
        void SetInput(IInput input);
    }
}