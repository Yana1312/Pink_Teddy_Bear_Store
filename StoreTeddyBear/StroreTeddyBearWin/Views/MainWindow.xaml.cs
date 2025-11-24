using Castle.Core.Resource;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using StoreTeddyBear.Controllers;
using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using StroreTeddyBearWin.Services;
using StroreTeddyBearWin.Views;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace StroreTeddyBearWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Toy currentToy;
        private List<Toy> toys = StorepinkteddybearBdContext.Instance.Toys.ToList();
        public MainWindow()
        {
            InitializeComponent();
        }
        private string _resetPasswordEmail;
        private string _resetCode;
        private string _generatedCode;

        private async void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string _password = GetCurrentPassword(PasswordAuthorizatoinTbox, PasswordAuthorizationPbox);

                var customer = StorepinkteddybearBdContext.Instance.Useransadmins.FirstOrDefault(cus => cus.EmailUsers == EmailTbox.Text);
                var errors = UserController.GetErrorsAuth(_password, EmailTbox.Text, customer);
                if (errors.Count > 0)
                {
                    MessageBox.Show($"Некорректные данные:\n\n{string.Join("\n", errors)}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                EnterBtn.IsEnabled = false;
                EnterBtn.Content = "Авторизация...";
                var res = await API.Auth(email: EmailTbox.Text, password: _password);
                if (res == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка" ,MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (res.RoleUsers == "пользователь")
                {
                    CatalogWindow catalog = new CatalogWindow(res);
                    catalog.Show();
                    this.Close();
                } else if (res.RoleUsers == "админ")
                {
                    AdminWindow admin = new AdminWindow();
                    admin.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка аворизации: {ex.Message}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnterBtn.IsEnabled = true;
                EnterBtn.Content = "Войти";
            }
        }

        private void ForgotPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordWindow.Visibility = Visibility.Visible;
            ForgotPasswordEmailTbox.Text = EmailTbox.Text;
            this.Title = "Восстановление пароля";
        }

        private void ReviewsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (toys == null || toys.Count == 0)
            {
                MessageBox.Show("Игрушек в магазине нет...");
                return;
            }
            Random random = new Random();
            int randomIndex = random.Next(0, toys.Count);

            ReviewWindow review = new ReviewWindow(toys[randomIndex], null);
            review.Show();
            this.Close();
        }

        private void CatalogBtn_Click(object sender, RoutedEventArgs e)
        {
            CatalogWindow catalogWindow = new CatalogWindow(null);
            catalogWindow.Show();
            this.Close();
        }

        private void RegistrationBtn_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow.Visibility = Visibility.Visible;
            this.Title = "Регистрация";
        }

        private async void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string _password = GetCurrentPassword(UserPasswordRegistrationTbox, UserPasswordRegistrationPbox);
                SignUpBtn.IsEnabled = false;
                SignUpBtn.Content = "      Регистрация...     ";

                var cus = Useransadmin.CreateUser(UserEmailRegistrationTbox.Text, UserNameRegistrationTbox.Text, _password);

                var errors = UserController.GetValidationErrors(cus);
                var existingCustomer = StorepinkteddybearBdContext.Instance.Useransadmins.FirstOrDefault(c =>
                                       c.EmailUsers.Equals(cus.EmailUsers));

                if (existingCustomer != null) errors.Add("Данная почта уже зарегистрирована");

                if (errors.Count > 0)
                {
                    MessageBox.Show($"Некорректные данные:\n\n{string.Join("\n", errors)}");
                    return;
                }
                var res = await API.Registration(email: UserEmailRegistrationTbox.Text, name: UserNameRegistrationTbox.Text, password: _password);

                if (res != null)
                {
                    MessageBox.Show($"Успешная регистрация! Добро пожаловать, {res.NameUsers}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    CatalogWindow catalog = new CatalogWindow(res);
                    catalog.Show();
                    this.Close();
                    UserEmailRegistrationTbox.Text = "";
                    UserNameRegistrationTbox.Text = "";
                    UserPasswordRegistrationPbox.Password = "";
                    UserPasswordRegistrationTbox.Text = "";
                    RegistrationWindow.Visibility = Visibility.Hidden;
                }
                else
                    MessageBox.Show("Не удалось зарегистрировать пользователя. Проверьте введенные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SignUpBtn.IsEnabled = true;
                SignUpBtn.Content = "Зарегистрироваться";

            }
        }

        private void CrossBtn_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow.Visibility = Visibility.Hidden;
            this.Title = "Авторизация";
        }

        private string GetCurrentPassword(TextBox passwordTb, PasswordBox passwordBox)
        {
            if (passwordTb.Visibility == Visibility.Visible)
                return passwordTb.Text;
            else
                return passwordBox.Password.ToString();
        }

        private void ShowPassword(TextBox passwordTbox, PasswordBox passwordPBox)
        {
            passwordTbox.Text = passwordPBox.Password;
            passwordPBox.Visibility = Visibility.Hidden;
            passwordTbox.Visibility = Visibility.Visible;
        }

        private void UnShowPassword(TextBox passwordTbox, PasswordBox passwordPBox)
        {
            passwordPBox.Password = passwordTbox.Text;
            passwordTbox.Visibility = Visibility.Hidden;
            passwordPBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Name == "ShowPasswordRegistrationCbox")
                ShowPassword(UserPasswordRegistrationTbox, UserPasswordRegistrationPbox);
            else if (checkBox != null && checkBox.Name == "ShowPasswordAuthorizationCbox")
                ShowPassword(PasswordAuthorizatoinTbox, PasswordAuthorizationPbox);
        }

        private void ShowPasswordCbox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Name == "ShowPasswordRegistrationCbox")
                UnShowPassword(UserPasswordRegistrationTbox, UserPasswordRegistrationPbox);
            else if (checkBox != null && checkBox.Name == "ShowPasswordAuthorizationCbox")
                UnShowPassword(PasswordAuthorizatoinTbox, PasswordAuthorizationPbox);
        }

        private void ForgotPasswordCrossBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetPasswordWindows();
        }
        private void ResetPasswordWindows()
        {
            ForgotPasswordWindow.Visibility = Visibility.Hidden;
            CodeGrid.Visibility = Visibility.Hidden;
            NewPaswordGrid.Visibility = Visibility.Hidden;
            this.Title = "Авторизация";

            ForgotPasswordEmailTbox.Text = "";
            CodeTbox.Text = "";
            NewPasswordTbox.Text = "";

            _resetPasswordEmail = null;
            _generatedCode = null;
        }

        private string GenerateRandomCode(int length = 6)
        {
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private readonly EmailService _emailService = new EmailService();
        private async void SendNewPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = ForgotPasswordEmailTbox.Text.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Введите адрес электронной почты", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Введите корректный адрес электронной почты", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                var user = StorepinkteddybearBdContext.Instance.Useransadmins
                    .FirstOrDefault(u => u.EmailUsers == email);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такой почтой не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SendNewPasswordBtn.IsEnabled = false;
                SendNewPasswordBtn.Content = "Отправка...";

                _generatedCode = GenerateRandomCode();
                _resetPasswordEmail = email;

                bool emailSent = await _emailService.SendPasswordResetEmail(email, _generatedCode, user.NameUsers);

                if (emailSent)
                {
                    MessageBox.Show("Код восстановления отправлен на вашу почту", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ForgotPasswordWindow.Visibility = Visibility.Hidden;
                    CodeGrid.Visibility = Visibility.Visible;
                    CodeTbox.Text = "";
                }
                else
                {
                    MessageBox.Show("Не удалось отправить письмо. Проверьте подключение к интернету.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке кода: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SendNewPasswordBtn.IsEnabled = true;
                SendNewPasswordBtn.Content = "Отправить";
            }
        }

        private void CheckCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string enteredCode = CodeTbox.Text.Trim();

                if (string.IsNullOrWhiteSpace(enteredCode))
                {
                    MessageBox.Show("Введите код из письма", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (enteredCode == _generatedCode)
                {
                    CodeGrid.Visibility = Visibility.Hidden;
                    NewPaswordGrid.Visibility = Visibility.Visible;
                    NewPasswordTbox.Text = "";
                }
                else
                {
                    MessageBox.Show("Неверный код восстановления", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке кода: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveNewPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newPassword = NewPasswordTbox.Text;

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("Введите новый пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPassword.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ChekNewPasswordBtn.IsEnabled = false;
                ChekNewPasswordBtn.Content = "Сохранение...";

                var user = StorepinkteddybearBdContext.Instance.Useransadmins
                    .FirstOrDefault(u => u.EmailUsers == _resetPasswordEmail);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var passwordUpdated = await API.EditProfile(user.IdCustomer, user.EmailUsers, newPassword, user.NameUsers);

                if (passwordUpdated != null)
                {
                    MessageBox.Show("Пароль успешно изменен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ResetPasswordWindows();
                }
                else
                {
                    MessageBox.Show("Не удалось изменить пароль. Попробуйте позже.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении пароля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ChekNewPasswordBtn.IsEnabled = true;
                ChekNewPasswordBtn.Content = "Сохранить";
            }
        }
    }
}