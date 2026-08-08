using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace TaskMangment.Utilities.Localization
{
    public static class LocalizationServiceCollectionExtensions
    {
        public static IServiceCollection AddTaskMangmentLocalization(this IServiceCollection services)
        {
            services.AddLocalization();

            // Default culture = Arabic
            var defaultCulture = new CultureInfo("ar");

            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            return services;
        }
    }
}
