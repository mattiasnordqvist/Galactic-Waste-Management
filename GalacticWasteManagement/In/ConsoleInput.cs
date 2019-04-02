using System;

namespace GalacticWasteManagement.In
{
    public class ConsoleInput : Input
    {
        public override void TrySet<T>(Param<T> param)
        {
            while (true)
            {
                Console.WriteLine($"Please provide value for {(param.optional ? "(optional) ":"")}parameter '{param.inputParam.Name}'.{(param.optional ? $" (default: '{param.defaultValue}')":"")}");
                var candidate = Console.ReadLine();
                try
                {
                    if(candidate==string.Empty && param.optional)
                    {
                        param.SetValue(param.defaultValue);
                        Console.WriteLine($"{param.inputParam.Name}={param.Value.Value}");
                        return;
                    }
                    param.SetValue(param.inputParam.Parse(candidate));
                    Console.WriteLine($"{param.inputParam.Name}={param.Value.Value}");
                    return;
                }
                catch(Exception e)
                {
                    Console.WriteLine($"'{candidate}' is not a valid value for '{param.inputParam.Name}'. {e.Message}");
                }
            }
        }
    }
}
