namespace TaskMangment.Infrastructure
{
    public class EmailSettings
    {
        public string From { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Task Manager";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string? BrevoApiKey { get; set; }

        /// <summary>
        /// When false, emails are not sent via Brevo/SMTP (queue still marks Sent). Default true for production.
        /// </summary>
        public bool SendEnabled { get; set; } = true;
    }
}
