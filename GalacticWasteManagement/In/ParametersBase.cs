﻿namespace GalacticWasteManagement
{
    public abstract class ParametersBase : IInput
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
    }
}