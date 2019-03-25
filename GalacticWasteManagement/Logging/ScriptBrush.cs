using ArtisticPastelPainter;

namespace GalacticWasteManagement.Logging
{
    public class ScriptBrush : ArtisticRegexBrush
    {
        public ScriptBrush() : base(@"'.*\.sql'", foreground: System.Drawing.Color.LightYellow)
        {
        }

        protected override void Unleash(ArtisticString coloredString, int index, int length)
        {
            base.Unleash(coloredString, index+1, length-2);
        }
    }
}
