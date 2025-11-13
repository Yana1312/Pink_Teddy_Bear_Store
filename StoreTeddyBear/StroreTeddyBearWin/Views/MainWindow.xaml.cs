using Castle.Core.Resource;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using StoreTeddyBear.Controllers;
using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
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
        public MainWindow()
        {
            InitializeComponent();
        }

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
                    MessageBox.Show("Пользователь не найден");
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
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
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

        }
        
        private void ReviewsBtn_Click(object sender, RoutedEventArgs e)
        {

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
                                       c.EmailUsers.Equals(cus.EmailUsers, StringComparison.OrdinalIgnoreCase));

                if (existingCustomer != null) errors.Add("Данная почта уже зарегистрирована");

                if (errors.Count > 0)
                {
                    MessageBox.Show($"Некорректные данные:\n\n{string.Join("\n", errors)}");
                    return;
                }
                var res = await API.Registration(email: UserEmailRegistrationTbox.Text, name: UserNameRegistrationTbox.Text, password: _password);

                if (res == null)
                {
                    MessageBox.Show($"Успешная регистрация! Добро пожаловать, {res.NameUsers}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    UserEmailRegistrationTbox.Text = "";
                    UserNameRegistrationTbox.Text = "";
                    UserPasswordRegistrationPbox.Password = "";
                    UserPasswordRegistrationTbox.Text = "";
                    RegistrationWindow.Visibility = Visibility.Hidden;
                }
                else
                    MessageBox.Show("Не удалось зарегистрировать пользователя. Проверьте введенные данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
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
    }
}