using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Infrastructure.Caching;

namespace TaskMangment.API.Extensions
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration config)
        {
            var enabled = config.GetValue<bool>("Redis:Enabled");

            if (!enabled)
            {
                services.AddScoped<ICachingService, NoCacheService>();
                return services;
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = config["Redis:ConnectionString"];
            });

            services.AddScoped<ICachingService, RedisCachingService>();
            return services;
        }
    }
}
