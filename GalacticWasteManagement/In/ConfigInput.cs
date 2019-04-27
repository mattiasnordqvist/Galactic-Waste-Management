using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GalacticWasteManagement.In
{
    public class ConfigInput : IInput
    {
        private IConfigurationSection configurationSection;

        public ConfigInput(IConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public string Name => "json";

        public void TrySet<T>(Param<T> param)
        {
            if (param.optional && !configurationSection.GetChildren().Any(x => x.Key == param.inputParam.Name))
            {
            }
            else
            {
                param.SetValue(param.inputParam.Parse(configurationSection[param.inputParam.Name]));
            }
        }
    }
}
