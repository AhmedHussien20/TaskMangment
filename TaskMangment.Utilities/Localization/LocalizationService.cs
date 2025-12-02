using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Utilities.Localization
{
    public class LocalizationService
    {
        private readonly ResourceManager _resourceManager;

        public LocalizationService()
        {
            _resourceManager = new ResourceManager("TaskMangment.Utilities.Localization.Resources.Messages", typeof(LocalizationService).Assembly);
        }

        public string Get(string key, string culture = "en")
        {
            var cultureInfo = new CultureInfo(culture);
            return _resourceManager.GetString(key, cultureInfo);
        }
    }
}
