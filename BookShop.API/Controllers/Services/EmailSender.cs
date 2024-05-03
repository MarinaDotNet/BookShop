using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.AspNetCore.Mvc;
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
    public class EmailSender(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;
        public async Task<ActionResult<bool>> SendEmailAsync(EmailAddress emailTo, string subject, string plainTextContent)
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
                if (emailTo.Email.Equals("admin@email.com") || emailTo.Email.Equals("user@email.com"))
        {
                    return false;
                }

                var client = new SendGridClient(_configuration[ApiConstants.SendGridApiKeyName]);

                var message = new SendGridMessage()
                {
                    From = new EmailAddress(_configuration[ApiConstants.SendEmailName], "Books Store team"),
                    Subject = subject,
                    PlainTextContent = plainTextContent,
                    HtmlContent = "<span>" + plainTextContent + "</span>"
                };
                message.AddTo(emailTo);
                var response = await client.SendEmailAsync(message).ConfigureAwait(false);

                Console.WriteLine("Email sended: " + response.StatusCode.ToString());
                return response.IsSuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            
            smtpClient.Send(mailMessage);
        }
    }
}
