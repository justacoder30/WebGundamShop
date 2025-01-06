using System.Net.Mail;
using System.Net;

namespace WebGundamShop.Areas.Admin.Repostitory
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
