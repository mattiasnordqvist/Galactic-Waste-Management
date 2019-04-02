using System;
using System.Collections.Generic;

namespace GalacticWasteManagement
{
    public class HardCodedInput : Input
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();

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
                    param.SetValueNull();
                }
                else
                {
                    throw new Exception($"Parameter {param.inputParam.Name} not set");
                }
            }
        }
    }
}