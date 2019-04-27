using System;
using System.Collections.Generic;

namespace GalacticWasteManagement
{
    public class Parameters : IInput, IParameters
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();
        private IInput input;
        private readonly GalacticWasteManager wasteManager;

        public string Name => "Parameters";

        public Parameters(IInput input, GalacticWasteManager wasteManager)
        {
            this.input = input;
            this.wasteManager = wasteManager;
        }

        public void SetInput(IInput input)
        {
            this.input = input;
        }

        public Param<T> Optional<T>(InputParam<T> inputParam, T defaultValue)
        {
            return new Param<T>(inputParam, defaultValue, true, this);
        }
        public Param<T> Required<T>(InputParam<T> inputParam)
        {
            return new Param<T>(inputParam, default, false, this);
        }

        public void TrySet<T>(Param<T> param)
        {
            var source = "";
            if (values.ContainsKey(param.inputParam.Name))
            {
                param.SetValue((T)values[param.inputParam.Name]);
                source = "code";
            }
            else
            {
                if (param.optional)
                {
                    if (input != null)
                    {
                        input.TrySet(param);
                        source = input.Name;
                    }
                    if (param.Value == null)
                    {
                        param.SetValue(param.defaultValue);
                        source = "default";
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (input == null)
                    {
                        throw new Exception($"Parameter {param.inputParam.Name} not set");
                    }
                    else
                    {
                        input.TrySet(param);
                        source = input.Name;
                    }
                }

            }
            wasteManager.Logger.Log($"Using parameter: {param.inputParam.Name}={param.Value.Value} (Source={source})", "info");
        }

        public void Supply(Dictionary<string, object> parameters)
        {
            foreach (var p in parameters ?? new Dictionary<string, object>())
            {
                values[p.Key] = p.Value;
            }
        }
    }
}