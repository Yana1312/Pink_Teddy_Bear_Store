using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using StoreTeddyBear.Controllers;
using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

           QuestPDF.Settings.License = LicenseType.Community;
        }

        private async void LoadUserData()
        {
            try
            {
                var orders = await API.GetCustomerOrders(CurrentUser.IdCustomer);
                _customerOrders = orders?.Where(o => o.StatusOrder != "ожидает подтверждения").ToList() ?? new List<Order>();
                OrdersCount = _customerOrders?.Count ?? 0;

                var reviews = await API.GetReviewsByCustomer(CurrentUser.IdCustomer);
                _customerReviews = reviews ?? new List<Review>();
                ReviewsCount = _customerReviews?.Count ?? 0;

                CurrentCart = await API.GetCart(CurrentUser.IdCustomer);
                CartItemsCount = CurrentCart?.Orderitems?.Count ?? 0;

                OrdersCountTb.Text = $"Всего заказов: {OrdersCount.ToString()}";
                ReviewsCountTb.Text = $"Оставлено отзывов: {ReviewsCount.ToString()}";
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
                var res = await API.EditProfile(id: CurrentUser.IdCustomer, email: UserEmailRegistrationTbox.Text, password: _password, name: UserNameRegistrationTbox.Text);

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

        private byte[] GenerateOrderPdf(Order order)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(20);

                        page.Header().Element(header =>
                        {
                            header.AlignCenter().
                            
                            Text("КАССОВЫЙ ЧЕК").Bold().FontSize(14);
                        });

                        page.Content().Element(content =>
                        {
                            content.PaddingVertical(10).Column(column =>
                            {
                                column.Spacing(8);

                                column.Item().Text($"Чек №: {order.IdOrder}");
                                column.Item().Text($"Дата: {order.DateOrder:dd.MM.yyyy HH:mm}");
                                column.Item().Text($"Клиент: {CurrentUser.NameUsers}");
                                column.Item().Text($"Email: {CurrentUser.EmailUsers}");
                                column.Item().Text($"Статус: {order.StatusOrder}");

                                column.Item().PaddingTop(10);
                                column.Item().Text("ТОВАРЫ:").Bold();

                                if (order.Orderitems != null && order.Orderitems.Any())
                                {
                                    foreach (var item in order.Orderitems)
                                    {
                                        string name = item.ArticulToyNavigation?.Title ?? "Товар";
                                        decimal price = (decimal)(item.ArticulToyNavigation?.Price ?? 0);
                                        int quantity = item.Quantity;
                                        decimal total = price * quantity;

                                        column.Item().PaddingTop(5).Row(row =>
                                        {
                                            row.RelativeItem(3).Text(name);
                                            row.RelativeItem().Text($"{quantity} x {price:F2}₽");
                                            row.RelativeItem().AlignRight().Text($"{total:F2}₽");
                                        });
                                    }
                                }
                                else
                                {
                                    column.Item().Text("Информация о товарах недоступна").Italic();
                                }

                                column.Item().PaddingTop(15);
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Общая стоимость:").Bold();
                                    row.RelativeItem().AlignRight().Text($"{order.TotalAmount:F2}₽").Bold();
                                });

                                column.Item().PaddingTop(20).AlignCenter().Text("Спасибо, что выбираете нас!").Italic();
                                column.Item().AlignCenter().Text("Store Teddy Bear").FontSize(10);
                            });
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        private void DownloadReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button?.Tag is Order order)
                {
                    byte[] pdfBytes = GenerateOrderPdf(order);
                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        MessageBox.Show("Не удалось сгенерировать чек", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "PDF файлы (*.pdf)|*.pdf",
                        FileName = $"Чек_заказа_{order.IdOrder}_{order.DateOrder:ddMMyyyy}.pdf",
                        Title = "Сохранить чек как"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, pdfBytes);
                        MessageBox.Show($"Чек успешно сохранен!\n{saveFileDialog.FileName}", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении чека: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendReceiptByEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button?.Tag is Order order)
                {
                    var result = MessageBox.Show($"Отправить чек заказа №{order.IdOrder} на email {CurrentUser.EmailUsers}?",
                                               "Отправка чека",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        byte[] pdfBytes = GenerateOrderPdf(order);
                        if (pdfBytes != null && SendPdfReceiptByEmail(CurrentUser.EmailUsers, pdfBytes, order.IdOrder.ToString()))
                        {
                            MessageBox.Show("Чек успешно отправлен на вашу почту!", "Успех",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось отправить чек на почту", "Ошибка",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке чека: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SendPdfReceiptByEmail(string toEmail, byte[] pdfBytes, string orderId)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("skincareaesthete@mail.ru"); 
                    mail.To.Add(toEmail);
                    mail.Subject = $"Кассовый чек №{orderId} - Store Teddy Bear";
                    mail.Body = $"Уважаемый(ая) {CurrentUser.NameUsers}!\n\n" +
                               $"В приложении находится кассовый чек для вашего заказа №{orderId}.\n\n" +
                               $"С уважением,\nStore Teddy Bear";
                    mail.IsBodyHtml = false;

                    using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
                    {
                        mail.Attachments.Add(new Attachment(pdfStream, $"Чек_заказа_{orderId}.pdf", "application/pdf"));

                        using (SmtpClient smtpClient = new SmtpClient("smtp.mail.ru")) 
                        {
                            smtpClient.Port = 587;
                            smtpClient.Credentials = new NetworkCredential("skincareaesthete@mail.ru", "your_password");
                            smtpClient.EnableSsl = true;

                            smtpClient.Send(mail);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки email: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}