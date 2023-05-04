using MailKit.Net.Smtp;
using MimeKit;

namespace Mails
{
    public class Program
    {
        public static void Main()
        {
            Console.Write("Email Address: ");
            var emailAddress = Console.ReadLine();
            Console.Write("In App Password: ");
            var password = Console.ReadLine();
            Console.Write("Display name: ");
            var displayname = Console.ReadLine();
            Console.Write("Subject: ");
            var subject = Console.ReadLine();
            Console.Write("Body Path: ");
            var bodyPathTxt = Console.ReadLine();
            if (bodyPathTxt != null)
            {
                using var reader = new StreamReader(bodyPathTxt);
                var body = reader.ReadToEnd();
                Console.Write("Number of attachments per email: ");

                var attachmentsPerEmail = int.Parse(Console.ReadLine() ?? string.Empty);
                Console.Write("Enter the file format: *pdf, docx, etc");
                var fileConfig = Console.ReadLine();
                Console.Write("Path of receivers mails txt file: ");
                var receiverMailsTxtPath = Console.ReadLine();
                Console.Write("Path of the txt file including attachments paths: ");
                var attachmentTxtPath = Console.ReadLine();
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
                    message.From.Add(new MailboxAddress(displayname, emailAddress));
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
                        var atta = new MimePart("application", fileConfig)
                        {
                            Content = new MimeContent(File.OpenRead(attachmentPaths[i * attachmentsPerEmail + j])),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            FileName = Path.GetFileName(attachmentPaths[i * attachmentsPerEmail + j])
                        };
                        multipart.Add(atta);
                    }

                    message.Body = multipart;
                    using SmtpClient client = new SmtpClient();
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate(emailAddress, password);
                    client.Send(message);
                    Console.WriteLine($"Email sent to {recipientEmails[i]}!");
                    client.Disconnect(true);
                }
                Console.WriteLine("All Good buddy!\npress any key to terminate ");
                Console.ReadKey();
            }
        }
    }
}