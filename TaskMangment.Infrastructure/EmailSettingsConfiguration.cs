using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaskMangment.Infrastructure
{
    public static class EmailSettingsConfiguration
    {
        public static IServiceCollection ConfigureEmailSettings(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<EmailSettings>(opts =>
            {
                configuration.GetSection("EmailSettings").Bind(opts);
                ApplyEnvironmentOverrides(opts);
            });

            return services;
        }

        public static void ApplyEnvironmentOverrides(EmailSettings opts)
        {
            var host = Environment.GetEnvironmentVariable("EMAIL_HOST");
            var user = Environment.GetEnvironmentVariable("EMAIL_HOST_USER");
            var password = Environment.GetEnvironmentVariable("EMAIL_HOST_PASSWORD");
            var from = Environment.GetEnvironmentVariable("DEFAULT_FROM_EMAIL");
            var brevoApiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");

            if (!string.IsNullOrWhiteSpace(host))
                opts.Host = host.Trim();

            if (!string.IsNullOrWhiteSpace(user))
                opts.User = user.Trim();

            if (!string.IsNullOrWhiteSpace(password))
                opts.Password = password.Trim();

            if (!string.IsNullOrWhiteSpace(from))
                opts.From = from.Trim();

            if (!string.IsNullOrWhiteSpace(brevoApiKey))
                opts.BrevoApiKey = brevoApiKey.Trim();

            var sendEnabled = Environment.GetEnvironmentVariable("EMAIL_SEND_ENABLED");
            if (bool.TryParse(sendEnabled, out var enabled))
                opts.SendEnabled = enabled;
        }
    }
}
