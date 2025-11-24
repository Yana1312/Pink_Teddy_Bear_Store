using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StroreTeddyBearWin.Services
{
    public class EmailService
    {
        public async Task<bool> SendPasswordResetEmail(string email, string code, string userName)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("skincareaesthete@mail.ru");
                    mail.To.Add(email);

                    mail.Subject = "Код восстановления пароля - Store Teddy Bear";
                    mail.Body = $"Уважаемый(ая) {userName}!\n\n" +
                               $"Для восстановления пароля используйте следующий код:\n\n" +
                               $"<h2 style=\"color: #EB85A7; font-size: 24px; text-align: center;\">{code}</h2>\n\n" +
                               $"Код действителен в течение 10 минут.\n\n" +
                               $"Если вы не запрашивали восстановление пароля, проигнорируйте это письмо.\n\n" +
                               $"С уважением,\nStore Teddy Bear";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtpClient = new SmtpClient("smtp.mail.ru"))
                    {
                        smtpClient.Port = 587;
                        smtpClient.Credentials = new NetworkCredential("skincareaesthete@mail.ru", "gegTfkutESlRo7IUMHe4");
                        smtpClient.EnableSsl = true;

                        await smtpClient.SendMailAsync(mail);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


