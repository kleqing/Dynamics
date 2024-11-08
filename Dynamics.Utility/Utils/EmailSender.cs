using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace Dynamics.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Because we're using app password, mail should be sent from me (Who register it in google)
            // Also, if email does not exist, the mail will be redirected into my mail box
            try
            {
                // My Google information
                string fromMail = "kietpmde180889@fpt.edu.vn";
                string fromPassword = @"pucw aypg zxws tjcb";
                SmtpClient smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com", // SMTP server
                    Port = 587, // TLS port
                    EnableSsl = true, // Enable TLS (SSL)
                    Credentials = new System.Net.NetworkCredential(fromMail, fromPassword) // MY Username and password
                };

                MailMessage mail = new MailMessage();
                mail.Subject = subject;
                mail.Body = GenerateEmailTemplate(email, subject, htmlMessage);

                //Setting From , To and CC
                mail.From = new MailAddress(fromMail);
                mail.To.Add(new MailAddress(email));
                //mail.CC.Add(new MailAddress("MyEmailID@gmail.com"));
                mail.IsBodyHtml = true; // Enable HTML
                smtpClient.Send(mail);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return Task.CompletedTask;
        }

        private string GenerateEmailTemplate(string email, string subject, string htmlMsg)
        {
            var result = string.Format(
                @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Verify Your Dynamics Account</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333333; margin: 0; padding: 0; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td style=""padding: 0;"">
                <table role=""presentation"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #0078D4; padding: 20px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px;"">Dynamics</h1>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 30px;"">
                            <h2 style=""color: #0078D4; margin-top: 0; margin-bottom: 20px; font-size: 24px;"">{1}</h2>
                            <p style=""margin-top: 0; margin-bottom: 20px;"">Hello, {0}</p>
                            <p style=""margin-top: 0; margin-bottom: 20px;"">{2}</p>
                            <p style=""margin-top: 0; margin-bottom: 20px;"">If you have any questions or need assistance, please don't hesitate to contact our support team at support@dynamics.com.</p>
                            <p style=""margin-top: 0; margin-bottom: 0;"">Best regards,<br>The Dynamics Team</p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f8f8; padding: 20px; text-align: center; font-size: 14px; color: #888888;"">
                            <p style=""margin: 0;"">This is an automated message, please do not reply to this email.</p>
                            <p style=""margin: 10px 0 0;"">© {DateTime.Now.Year} Dynamics. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
", email, subject, htmlMsg);
            return result;
        }
    }
}
