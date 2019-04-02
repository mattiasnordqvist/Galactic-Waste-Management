using System;
using System.Collections.Generic;

namespace GalacticWasteManagement
{
    public class HardCodedInput : Input
    {
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        private Dictionary<string, object> values = new Dictionary<string, object>();
        public override Param<T> Optional<T>(InputParam<T> inputParam, T defaultValue)
        {
            parameters[inputParam.Name] = new Param<T>(inputParam, defaultValue, true, this);
            return (Param<T>)parameters[inputParam.Name];
        }

        public override Param<T> Required<T>(InputParam<T> inputParam)
        {
            parameters[inputParam.Name] = new Param<T>(inputParam, default(T), false, this);
            return (Param<T>)parameters[inputParam.Name];
        }

        public void Set<T>(string paramName, T value)
        {
            values[paramName] = value;
        }

        public override void Set<T>(Param<T> param)
        {
            if (values.ContainsKey(param.inputParam.Name))
            {
                var p = (Param<T>)parameters[param.inputParam.Name];
                p.SetValue((T)values[param.inputParam.Name]);
            }
            else
            {
                throw new Exception($"Parameter {param.inputParam.Name} not set");
            }
        }
    }
}