using System;
using System.Collections.Generic;

namespace GalacticWasteManagement
{

    public class Parameters : ParametersBase
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();
        private IInput input;

        public Parameters(IInput input)
        {
            this.input = input;
        }

        public void SetInput(IInput input)
        {
            this.input = input;
        }

        public override void TrySet<T>(Param<T> param)
        {
            if (values.ContainsKey(param.inputParam.Name))
            {
                param.SetValue((T)values[param.inputParam.Name]);
            }
            else
            {
                if (param.optional)
                {
                    input.TrySet(param);
                    if (!param.Value.HasValue)
                    {
                        param.SetValue(param.defaultValue);
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
                    }
                }
            }
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