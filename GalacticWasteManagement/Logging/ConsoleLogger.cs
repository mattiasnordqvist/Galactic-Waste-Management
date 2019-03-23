using System;
using System.Drawing;
using ArtisticPastelPainter;

namespace GalacticWasteManagement.Logging
{
    public class ConsoleLogger : ILogger
    {
        ArtisticPainter painter = new ArtisticPainter();
        public ConsoleLogger(string databaseName)
        {
            painter.BeCreativeWith(new ArtisticRegexBrush(databaseName, Color.Orange));
        }
        public void Log(string message, string type)
        {
            var defaultColor = type == "success"
                ? Color.Green
                : type == "warning"
                    ? Color.Yellow
                    : type == "error"
                        ? Color.Red
                        : type == "important"
                        ? Color.White
                        : Color.DimGray;
            Console.WriteLine(painter.Unleash(new ArtisticString(message, defaultColor)));
        }
    }
}
