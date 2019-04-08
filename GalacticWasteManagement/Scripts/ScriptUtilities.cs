using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GalacticWasteManagement.Scripts
{
    public static class ScriptUtilities
    {
        public static List<string> SplitInBatches(string @this)
        {
            Regex batchRegex = new Regex("\\b([Gg][Oo])\\b;?");
            return batchRegex
                    .Split(@this)
                    .Select(x => x.Trim())
                    .Where(x => !batchRegex.IsMatch(x))
                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

        }

        public static string ReplaceTokens(string @this, IDictionary<string, string> variables)
        {
            foreach (var variable in variables)
            {
                @this = @this.Replace($"${variable.Key}$", variable.Value);
            }
            return @this;
        }
    }
}
