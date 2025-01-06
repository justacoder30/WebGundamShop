using System.Net.Mail;
using System.Net;

namespace WebGundamShop.Areas.Admin.Repostitory
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("dangquang0129@gmail.com", "csfhxeenjoyzbnxy")
            };

            return client.SendMailAsync(
                new MailMessage(from: "dangquang0129@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
