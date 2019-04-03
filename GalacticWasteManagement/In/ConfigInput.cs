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

        public void TrySet<T>(Param<T> param)
        {
            if (param.optional && !configurationSection.GetChildren().Any(x => x.Key == param.inputParam.Name))
            {
                param.SetValue(param.defaultValue);
            }
            else
            {
                param.SetValue(param.inputParam.Parse(configurationSection[param.inputParam.Name]));
            }
        }
    }
}
