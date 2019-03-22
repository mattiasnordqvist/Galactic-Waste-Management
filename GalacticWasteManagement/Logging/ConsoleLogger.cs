using System;

namespace GalacticWasteManagement.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message, string type)
        {
            var color = type == "success" 
                ? ConsoleColor.Green
                : type == "warning" 
                    ? ConsoleColor.Yellow
                    : type == "error"
                        ? ConsoleColor.Red
                        : ConsoleColor.White;
            Console.WriteLine(message, color);
        }
    }
}
