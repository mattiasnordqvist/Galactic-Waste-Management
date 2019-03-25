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
            painter
                .BeCreativeWith(new ArtisticRegexBrush(databaseName, Color.Orange))
                .BeCreativeWith(new ScriptBrush());
        }
        public void Log(string message, string type)
        {
            var defaultBrush = type == "unicorn"
                ? (IArtisticBrush)new RainbowBrush()
                : type == "success"
                ? new BasicBrush(Color.YellowGreen)
                : type == "warning"
                ? new BasicBrush(Color.Yellow)
                : type == "error"
                ? new BasicBrush(Color.Red)
                : type == "important"
                ? new BasicBrush(Color.White)
                : new BasicBrush(Color.DimGray);
            Console.WriteLine(painter.Unleash(new ArtisticString(message, defaultBrush)));
        }
    }
}
