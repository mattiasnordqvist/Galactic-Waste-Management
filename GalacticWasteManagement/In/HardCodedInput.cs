using System;
using System.Collections.Generic;

namespace GalacticWasteManagement
{
    public class HardCodedInput : Input
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();
        private Input backUpInput;

        public HardCodedInput(Dictionary<string, object> parameters, Input backUpInput = null)
        {
            values = parameters ?? new Dictionary<string, object>();
            this.backUpInput = backUpInput;
        }

        public void Set<T>(string paramName, T value)
        {
            values[paramName] = value;
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
                    backUpInput.TrySet(param);
                    if (!param.Value.HasValue)
                    {
                        param.SetValue(param.defaultValue);
                    }
                }
                else
                {
                    if (backUpInput == null)
                    {
                        throw new Exception($"Parameter {param.inputParam.Name} not set");
                    }
                    else
                    {
                        backUpInput.TrySet(param);
                    }
                }
            }
        }
    }
}