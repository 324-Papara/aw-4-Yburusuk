using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Para.Bussiness.Notification;

public class NotificationService  : INotificationService
{
    private readonly IConfiguration configuration;

    public NotificationService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }
    public void SendEmail(string subject, string email, string content)
    {
        var smtpSettings = configuration.GetSection("SmtpSettings");

        SmtpClient smtpClient = new SmtpClient(smtpSettings["Host"])
        {
            Port = int.Parse(smtpSettings["Port"]),
            Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
            EnableSsl = true
        };

        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(smtpSettings["From"]),
            Subject = subject,
            Body = content,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);
        smtpClient.Send(mailMessage);
    }
}