using System;
using System.Collections.Generic;

namespace GalacticWasteManagement
{

    public abstract class Input
    {
        public Param<T> Optional<T>(InputParam<T> inputParam, T defaultValue)
        {
            return new Param<T>(inputParam, defaultValue, true, this);
        }
        public Param<T> Required<T>(InputParam<T> inputParam)
        {
            return new Param<T>(inputParam, default, false, this);
        }
        public abstract void TrySet<T>(Param<T> param);

        public abstract void Supply(Dictionary<string, object> parameters);
    }
}