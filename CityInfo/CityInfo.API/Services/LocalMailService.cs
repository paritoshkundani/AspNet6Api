namespace CityInfo.API.Services
{
    public class LocalMailService : IMailService
    {
        public readonly string _mailTo = string.Empty;
        public readonly string _mailFrom = string.Empty;

        // IConfiguration is already setup by .net so don't need to add it to Program.cs
        public LocalMailService(IConfiguration configuration)
        {
            _mailTo = configuration["mailSettings:mailToAddress"];
            _mailFrom = configuration["mailSettings:mailFromAddress"];
        }

        public void Send(string subject, string message)
        {
            // send mail - output to console window
            Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, " +
                $"with {nameof(LocalMailService)}.");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: { message}");
        }
    }
}
