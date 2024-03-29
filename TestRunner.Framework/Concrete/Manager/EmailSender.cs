﻿using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace TestRunner.Framework.Concrete.Manager
{
    public class EmailSender
    {
        private readonly string _mailFrom;
        private readonly string _mailSmtpHost;
        private readonly string _mailSmtpPassword;
        private readonly int _mailSmtpPort;
        private readonly string _mailSmtpUsername;

        
        public EmailSender(string mailSmtpHost, int mailSmtpPort, string mailSmtpUsername, string mailSmtpPassword,
            string mailFrom)
        {
            _mailSmtpHost = mailSmtpHost;
            _mailSmtpPort = mailSmtpPort;
            _mailSmtpUsername = mailSmtpUsername;
            _mailSmtpPassword = mailSmtpPassword;
            _mailFrom = mailFrom;
        }

        public bool SendEmail(string to, string subject, string body)
        {
            var mail = new MailMessage(_mailFrom, to, subject, body);
            AlternateView alternameView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
            mail.AlternateViews.Add(alternameView);

            var smtpClient = new SmtpClient(_mailSmtpHost, _mailSmtpPort)
            {
                Credentials = new NetworkCredential(_mailSmtpUsername, _mailSmtpPassword),
                EnableSsl = true
            };

            smtpClient.Send(mail);
            return true;
        }

        public bool SendEmail(string to, string subject, string body, Attachment attachment)
        {
            var mail = new MailMessage(_mailFrom, to, subject, body);
            AlternateView alternameView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
            mail.AlternateViews.Add(alternameView);
            mail.Attachments.Add(attachment);

            var smtpClient = new SmtpClient(_mailSmtpHost, _mailSmtpPort)
            {
                Credentials = new NetworkCredential(_mailSmtpUsername, _mailSmtpPassword),
                EnableSsl = true
            };

            smtpClient.Send(mail);
            return true;
        }

        public bool SendEmail(string to, string subject, string body, string cc)
        {
            var mail = new MailMessage(_mailFrom, to, subject, body);
            AlternateView alternameView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
            mail.AlternateViews.Add(alternameView);
            mail.CC.Add(cc);

            var smtpClient = new SmtpClient(_mailSmtpHost, _mailSmtpPort)
            {
                Credentials = new NetworkCredential(_mailSmtpUsername, _mailSmtpPassword),
                EnableSsl = true
            };

            smtpClient.Send(mail);
            return true;
        }

        public bool SendEmail(string to, string subject, string body, string cc, Attachment attachment)
        {
            var mail = new MailMessage(_mailFrom, to, subject, body);
            AlternateView alternameView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
            mail.AlternateViews.Add(alternameView);
            mail.CC.Add(cc);
            mail.Attachments.Add(attachment);

            var smtpClient = new SmtpClient(_mailSmtpHost, _mailSmtpPort)
            {
                Credentials = new NetworkCredential(_mailSmtpUsername, _mailSmtpPassword),
                EnableSsl = true
            };

            smtpClient.Send(mail);
            return true;
        }

        public bool SendEmail(string to, string subject, string body, List<string> ccsList)
        {
            var mail = new MailMessage(_mailFrom, to, subject, body);
            AlternateView alternameView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
            mail.AlternateViews.Add(alternameView);

            foreach (string cc in ccsList)
            {
                mail.CC.Add(cc);
            }

            var smtpClient = new SmtpClient(_mailSmtpHost, _mailSmtpPort)
            {
                Credentials = new NetworkCredential(_mailSmtpUsername, _mailSmtpPassword),
                EnableSsl = true
            };

            smtpClient.Send(mail);
            return true;
        }

       
    }
}