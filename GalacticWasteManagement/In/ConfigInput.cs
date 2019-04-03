using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace GalacticWasteManagement.In
{
    public class ConfigInput : Input
    {
        private IConfigurationSection configurationSection;

        public ConfigInput(IConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Supply(Dictionary<string, object> parameters)
        {
            throw new System.NotImplementedException();
        }

        public override void TrySet<T>(Param<T> param)
        {
            if (param.optional && !configurationSection.GetChildren().Any(x => x.Key == param.inputParam.Name))
            {
                param.SetValueNull();
            }
            else
            {
                param.SetValue(param.inputParam.Parse(configurationSection[param.inputParam.Name]));
            }
        }
    }
}
