using GalacticWasteManagement.Logging;
using SuperNotUnderstandableInputHandling;
using System.Collections.Generic;

namespace GalacticWasteManagement
{
    public class LoggingParameters : IInput, IParameters
    {
        private readonly Parameters adaptee;
        private readonly ILogger logger;

        public LoggingParameters(Parameters adaptee, ILogger logger)
        {
            this.adaptee = adaptee;
            this.logger = logger;
        }

        public string Name => adaptee.Name;

        public Param<T> Optional<T>(InputParam<T> inputParam, T defaultValue)
        {
            return adaptee.Optional(inputParam, defaultValue);
        }

        public Param<T> Required<T>(InputParam<T> inputParam)
        {
            return adaptee.Required(inputParam);
        }

        public void SetInput(IInput input)
        {
            adaptee.SetInput(input);
        }

        public void Supply(Dictionary<string, object> parameters)
        {
            adaptee.Supply(parameters);
        }

        public TrySetResult<T> TrySet<T>(Param<T> param)
        {
            var result = adaptee.TrySet(param);
            logger.Log($"Using parameter: {result.Name}={result.Value} (Source={result.Source})", "info");
            return result;
        }
    }
}