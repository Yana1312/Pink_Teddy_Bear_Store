using StoreTeddyBear.Controllers;
using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;

namespace StroreTeddyBearWin.Views
{
    public partial class ProfileWindow : Window
    {
        public Useransadmin CurrentUser;
      

        public string UserInitials =>
            CurrentUser?.NameUsers?.Length > 0
            ? CurrentUser.NameUsers.Substring(0, 1).ToUpper()
            : "U";

        private List<Order> _customerOrders;

        private List<Review> _customerReviews; 

        public int OrdersCount;
        public int ReviewsCount;
        public int CartItemsCount;

        private Order CurrentCart;

        public ProfileWindow(Useransadmin user)
        {
            InitializeComponent();
            CurrentUser = user;
            DataContext = CurrentUser;
            StartNameTb.Text = UserInitials;
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                var orders = await API.GetCustomerOrders(CurrentUser.IdCustomer);
                _customerOrders = orders ?? new List<Order>();
                OrdersCount = _customerOrders?.Count ?? 0;

                var reviews = await API.GetReviewsByCustomer(CurrentUser.IdCustomer);
                _customerReviews = reviews ?? new List<Review>();
                ReviewsCount = _customerReviews?.Count ?? 0;

                CurrentCart = await API.GetCart(CurrentUser.IdCustomer);
                CartItemsCount = CurrentCart?.Orderitems?.Count ?? 0;

                OrdersCountTb.Text = $"Всего заказов: {OrdersCount.ToString()}";
                ReviewsCountTb.Text = $"Оставлено отзывов: {ReviewsCount.ToString()}";
                CartItemsCountTb.Text = $"Активная корзина: {CartItemsCount.ToString()}";
                OrdersItemsControl.ItemsSource = _customerOrders;
                ReviewsItemsControl.ItemsSource = _customerReviews;

                NoOrdersText.Visibility = OrdersCount == 0 ? Visibility.Visible : Visibility.Collapsed;
                NoReviewsText.Visibility = ReviewsCount == 0 ? Visibility.Visible : Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CrossBtn_Click(object sender, RoutedEventArgs e)
        {
            EditGrid.Visibility = Visibility.Hidden;
            this.Title = "Профиль пользователя";
        }

        private async void EditProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            EditGrid.Visibility = Visibility.Visible;
            this.Title = "Редактирование данных";
        }

        private async void DeactivateBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите деактивировать аккаунт?",
                                       "Подтверждение",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var deactivatedUser = await API.DeactivateCustomer(CurrentUser.IdCustomer);
                if (deactivatedUser != null)
                {
                    CurrentUser = deactivatedUser;
                    MessageBox.Show("Аккаунт успешно деактивирован", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при деактивации аккаунта", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CatalogBtn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CatalogWindow catalogWindow = new CatalogWindow(CurrentUser);
            catalogWindow.Show();
            this.Close();
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
        }

        private void ShowPasswordCbox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Name == "ShowPasswordRegistrationCbox")
                UnShowPassword(UserPasswordRegistrationTbox, UserPasswordRegistrationPbox);
        }

        private async void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string _password = GetCurrentPassword(UserPasswordRegistrationTbox, UserPasswordRegistrationPbox);
                EditBtn.IsEnabled = false;
                EditBtn.Content = "      Редактирование...     ";

                var cus = Useransadmin.CreateUser(UserEmailRegistrationTbox.Text, UserNameRegistrationTbox.Text, _password);

                var errors = UserController.GetValidationErrors(cus);
                if (errors.Count > 0)
                {
                    MessageBox.Show($"Некорректные данные:\n\n{string.Join("\n", errors)}");
                    return;
                }
                var res = await API.EditProfile(id: CurrentUser.IdCustomer, email: UserEmailRegistrationTbox.Text, name: UserNameRegistrationTbox.Text, password: _password);

                if (res != null)
                {
                    MessageBox.Show($"Успешное редактирование данных!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    EditGrid.Visibility = Visibility.Hidden;
                    UserPasswordRegistrationPbox.Password = "";
                    UserPasswordRegistrationTbox.Text = "";
                    CurrentUser = res;
                    DataContext = CurrentUser;
                }
                else
                    MessageBox.Show("Не удалось обновить данные. Проверьте введенные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EditBtn.IsEnabled = true;
                EditBtn.Content = "Отредактировать...";
            }
        }
    }
}