using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StroreTeddyBearWin.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _fromEmail = "a32053560@gmail.com"; 
        private readonly string _fromPassword = "hwdg hakh htjs isqm";

        public async Task<bool> SendPasswordResetEmail(string toEmail, string Password, string userName)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                    client.Timeout = 30000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail, "Pink Bear Store"),
                        Subject = "Восстановление пароля",
                        Body = CreateEmailBody(userName, Password),
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private string CreateEmailBody(string userName, string newPassword)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #FFFAC0D5, #FFEB85A7); padding: 20px; text-align: center; border-radius: 10px; }}
                    .content {{ padding: 20px; background: #fff; }}
                    .password {{ font-size: 18px; font-weight: bold; color: #E91E63; padding: 10px; background: #FFF4F4; border-radius: 5px; text-align: center; }}
                    .warning {{ color: #ff6b6b; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Pink Bear Store</h1>
                    </div>
                    <div class='content'>
                        <h2>Восстановление пароля</h2>
                        <p>Здравствуйте, <strong>{userName}</strong>!</p>
                        <p>Вы запросили восстановление пароля для вашего аккаунта.</p>
                        <p>Ваш пароль:</p>
                        <div class='password'>{newPassword}</div>
                        <p>С уважением,<br>Команда Pink Bear Store</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}