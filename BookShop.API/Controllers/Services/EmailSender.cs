using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.AspNetCore.Mvc;
using DnsClient;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Sockets;
/*
 * Uses use the Twilio SendGrid Web API v3 to send emails
 * 
 * If email sended successfully returns true and if not returns false
 * 
 * If the requested, to send message, on email address is one of defoults(admin@email.com or user@email.com) 
 * just returns false without sending
 */

//TODO add check to SendGrid how many free emails left, if non left may be then to use smtp Client
namespace BookShop.API.Controllers.Services
{
    public class EmailSender(IConfiguration configuration) : IEmailSender
    {
        private readonly IConfiguration _configuration = configuration;

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
                ////send with smtp Client
                //MailMessage mailMessage = new();
                //SmtpClient smtpClient = new();
                //smtpClient.UseDefaultCredentials = false;
                //smtpClient.Credentials = new NetworkCredential("msichova.net@gmail.com", "gqim itbt jity evzk");
                //smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                //smtpClient.Host = "smtp.gmail.com";
                //smtpClient.EnableSsl = true;
                //smtpClient.Port = 587;
                //mailMessage.From = new MailAddress("msichova.net@gmail.com");
                //mailMessage.To.Add(new MailAddress("msichova@outlook.com"));
                //mailMessage.Subject = "To Try send email";
                //mailMessage.IsBodyHtml = true;
                //mailMessage.Body = "Just trying smtp client";
                //smtpClient.Send(mailMessage);

                try
                {
                    //Checking email address if it is default email, then it is impossible to send to it email
                    if (email.Equals("admin@msichova.com") || email.Equals("user@msichova.com"))
                    {
                        return Task.FromException(new OperationCanceledException());
                    }

                    //Checking if email address has a valid host name
                    string hostName = email.Split('@').ElementAt(1);
                    var lookup = new LookupClient();
                    var mxRecords = lookup.Query(hostName, QueryType.MX);
                    if (!mxRecords.Answers.MxRecords().Any())
                    {
                        return Task.FromException(new SocketException());
                    }

                    var client = new SendGridClient(_configuration[ApiConstants.SendGridApiKeyName]);

                    var message = new SendGridMessage()
                    {
                        From = new EmailAddress(_configuration[ApiConstants.SendEmailName], "Books Store team"),
                        Subject = subject,
                        PlainTextContent = htmlMessage,
                        HtmlContent = "<span>" + htmlMessage + "</span>"
                    };
                    message.AddTo(email);
                    var response = client.SendEmailAsync(message).ConfigureAwait(false);

                    Console.WriteLine("Email sended: " + response.ToString());

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return Task.FromException(ex);
                }
            }
        }
}
