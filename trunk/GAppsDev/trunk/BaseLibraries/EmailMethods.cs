using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace BaseLibraries
{
    public class EmailMethods
    {
        MailAddress fromAddress;
        string fromPassword;

        public EmailMethods(string fromEmail, string fromName, string fromSecurePassword)
        {
            fromAddress = new MailAddress(fromEmail, fromName);
            fromPassword = fromSecurePassword;
        }

        public bool sendGoogleEmail(string email, string fullName, string subject, string body, byte[] attachmentFile = null, string attachmentName = null)
        {
            MailAddress toAddress = new MailAddress(email, fullName);
            string mailSubject = subject;
            string mailBody = body;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                if (attachmentFile != null)
                {
                    Stream stream = new MemoryStream(attachmentFile);

                    Attachment attachment = new Attachment(stream, attachmentName);
                    message.Attachments.Add(attachment);
                    smtp.Send(message);

                    stream.Close();
                }
                else
                {
                    smtp.Send(message);
                }

                return true;
            }
        }
    }
}
