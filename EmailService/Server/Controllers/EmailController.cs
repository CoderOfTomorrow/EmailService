using EmailService.Shared;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Collections.Generic;

namespace EmailService.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public EmailController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public IActionResult SentEmail([FromBody] EmailDto email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(configuration["Email:Name"], configuration["Email:Username"]));
            message.To.Add(new MailboxAddress(email.Name, email.Name));
            message.Subject = email.Subject;

            message.Body = new TextPart("plain")
            {
                Text = email.Content
            };

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect("smtp.gmail.com", 465, true);
                smtpClient.Authenticate(configuration["Email:Username"], configuration["Email:Password"]);
                smtpClient.Send(message);
                smtpClient.Disconnect(true);
            }

            return Ok();
        }

        [HttpGet]
        public List<EmailDto> ReciveEmails()
        {
            var emailList = new List<EmailDto>();

            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, true);

                client.Authenticate(configuration["Email:Username"], configuration["Email:Password"]);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                for (int i = inbox.Count - 1; i > inbox.Count - 11; i--)
                {
                    var message = inbox.GetMessage(i);
                    var newEmail = new EmailDto
                    {
                        Name = message.From.ToString(),
                        Subject = message.Subject
                    };
                    emailList.Add(newEmail);
                }

                client.Disconnect(true);
            }

            return emailList;
        }
    }
}
