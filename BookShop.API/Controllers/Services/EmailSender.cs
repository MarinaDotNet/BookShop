using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;

namespace BookShop.API.Controllers.Services
{
    public class EmailSender
    {
        public void SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage mailMessage = new();
            SmtpClient smtpClient = new();

            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("msichova.net@gmail.com", "O424o39989");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.EnableSsl = true;
            smtpClient.Port = 587;

            mailMessage.From = new MailAddress("msichova.net@gmail.com");
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = htmlMessage;

            
            smtpClient.Send(mailMessage);
        }
    }
}
