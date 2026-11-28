namespace TaskMangment.Infrastructure
{
    public class WhatsAppSettings
    {
        public string InstanceId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string FrontendUrl { get; set; } = string.Empty;

        /// <summary>
        /// When false, WhatsApp is not sent (callers still treat as success). Default true for production.
        /// </summary>
        public bool SendEnabled { get; set; } = true;
    }
}
