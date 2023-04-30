using System;
using System.IO;
using MailKit.Net.Smtp;
using MimeKit;

namespace Mails
{
    public class Program
    {
        public static void Main()
        {
            string? emailAddress, password, subject, bodyPathTxt, receiverMailsTxtPath, attachmentTxtPath, body;
            int attachmentsPerEmail;
            Console.Write("Email Address: ");
            emailAddress = Console.ReadLine();
            Console.Write("In App Password: ");
            password = Console.ReadLine();
            Console.Write("Subject: ");
            subject = Console.ReadLine();
            Console.Write("Body Path: ");
            bodyPathTxt = Console.ReadLine();
            if (bodyPathTxt != null)
            {
                using var reader = new StreamReader(bodyPathTxt);
                body = reader.ReadToEnd();
                Console.Write("Number of attachments per email: ");

                attachmentsPerEmail = int.Parse(Console.ReadLine() ?? string.Empty);
                Console.Write("Path of receivers mails txt file: ");
                receiverMailsTxtPath = Console.ReadLine();
                Console.Write("Path of the txt file including attachments paths: ");
                attachmentTxtPath = Console.ReadLine();
                if (attachmentTxtPath == null || receiverMailsTxtPath == null)
                {
                    throw new ArgumentNullException(
                        "you should enter the receiver mails txt path and/or the attachment txt path");
                }
                string[] recipientEmails = File.ReadAllLines(receiverMailsTxtPath);
                string[] attachmentPaths = File.ReadAllLines(attachmentTxtPath);
                for (int i = 0; i < recipientEmails.Length; i++)
                {
                    MimeMessage message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Ozzy", emailAddress));
                    message.To.Add(MailboxAddress.Parse(recipientEmails[i]));
                    message.Subject = subject;
                    var bodyMultipart = new Multipart("plain");
                    var bodyText = new TextPart("plain")
                    {
                        Text = body
                    };
                    bodyMultipart.Add(bodyText);
                    var multipart = new Multipart("mixed");
                    multipart.Add(bodyMultipart);

                    for (int j = 0; j < attachmentsPerEmail; j++)
                    {
                        var atta = new MimePart("application", "pdf")
                        {
                            Content = new MimeContent(File.OpenRead(attachmentPaths[i * attachmentsPerEmail + j])),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            FileName = Path.GetFileName(attachmentPaths[i * attachmentsPerEmail + j])
                        };
                        multipart.Add(atta);
                    }

                    message.Body = multipart;
                    using (SmtpClient client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 465, true);
                        client.Authenticate(emailAddress, password);
                        client.Send(message);
                        Console.WriteLine($"Email sent to {recipientEmails[i]}!");
                        client.Disconnect(true);
                    }
                }

                Console.WriteLine("All Good buddy!");
            }
        }
    }
}