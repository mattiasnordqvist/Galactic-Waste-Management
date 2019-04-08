using GalacticWasteManagement.SqlServer;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GalacticWasteManagement.Scripts
{
    public static class ScriptUtilities
    {
        public static List<string> SplitInBatches(string script)
        {
            return new MsSqlScriptParser().SplitInBatches(script)
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
