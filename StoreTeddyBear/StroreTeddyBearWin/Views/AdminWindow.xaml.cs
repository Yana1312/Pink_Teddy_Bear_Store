using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using StoreTeddyBear.Controllers;
using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StroreTeddyBearWin.Views
{
    public partial class AdminWindow : Window
    {
        private List<Toy> _allToys;
        private List<Order> _allOrders;
        private Toy _selectedToy;
        private bool _isEditMode = false;
        private string _selectedImagePath = "";
        public AdminWindow()
        {
            InitializeComponent();
            LoadToys();
            LoadAllOrders();
        }

        private async void LoadToys()
        {
            try
            {
                using (var context = new StorepinkteddybearBdContext())
                {
                    _allToys = context.Toys.ToList();
                    ToysItemsControl.ItemsSource = _allToys;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private async void LoadAllOrders()
        {
            try
            {
                using (var context = new StorepinkteddybearBdContext())
                {
                    _allOrders = await context.Orders
                        .Include(o => o.Orderitems)
                        .ThenInclude(oi => oi.ArticulToyNavigation)
                        .OrderByDescending(o => o.DateOrder)
                        .ToListAsync();

                    OrdersItemsControl.ItemsSource = _allOrders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
            
        }

        private void FilterOrders()
        {
            if (_allOrders == null) return;

            var filteredOrders = _allOrders.AsEnumerable();

            if (StatusFilterComboBox.SelectedItem is ComboBoxItem statusItem &&
                statusItem.Content.ToString() != "Все статусы")
            {
                //if (statusItem.Content.ToString() == "доставлен" || statusItem.Content.ToString() == "ожидает подтверждения")
                //    ChangeStatusCb.Visibility = Visibility.Hidden;

                string selectedStatus = statusItem.Content.ToString();
                filteredOrders = filteredOrders.Where(o => o.StatusOrder == selectedStatus);
            }

            OrdersItemsControl.ItemsSource = filteredOrders.ToList();

        }

       

        private void RefreshOrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadAllOrders();
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void ProfileImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private async void DeleteBearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedToy == null)
            {
                MessageBox.Show("Выберите товар для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить товар '{_selectedToy.Title}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await API.DeleteToy(_selectedToy.ArticulToy);

                    if (success)
                    {
                        MessageBox.Show("Товар успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadToys();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить товар. Возможно, он есть в активных заказах.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddBearBtn_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            ClearForm();
            AddBearGrid.Visibility = Visibility.Visible;
            SetUpBearBtn.Content = "Добавить";
        }

        private void RedacBearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedToy == null)
            {
                MessageBox.Show("Выберите товар для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = true;
            LoadToyToForm(_selectedToy);
            AddBearGrid.Visibility = Visibility.Visible;
            SetUpBearBtn.Content = "Сохранить изменения";
        }

        private string CopyImageToProject(string sourcePath, string toyTitle)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourcePath) && File.Exists(sourcePath))
                {
                    string imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "ElementsVisualization", "Bears");

                    string safeFileName = string.Join("_", toyTitle.Split(Path.GetInvalidFileNameChars()));
                    string fileExtension = Path.GetExtension(sourcePath);
                    string destinationFileName = $"{safeFileName}{fileExtension}";
                    string destinationPath = Path.Combine(imageFolder, destinationFileName);

                    File.Copy(sourcePath, destinationFileName, overwrite: true);

                    return $"/ElementsVisualization/Bears/{destinationFileName}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка копирования изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); ;
            }
            return null;
        }
        
        private void GetPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;)|*.png;|All files (*.*)|*.*",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                PathToImageTbox.Text = _selectedImagePath;
                BearNewImage.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }

        private async void SetUpBearBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string imageFileName = null;
                if(!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
                {
                    string toyTitle = UserNameRegistrationTbox.Text;
                    if (string.IsNullOrEmpty(toyTitle))
                    {
                        MessageBox.Show("Введите название игрушки перед сохранением изображения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    imageFileName = CopyImageToProject(_selectedImagePath, toyTitle);
                    if (imageFileName == null)
                    {
                        MessageBox.Show("Не удалось сохранить изображение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }


                if (_isEditMode)
                {
                    var updatedToy = new Toy
                    {
                        ArticulToy = _selectedToy.ArticulToy,
                        Title = UserNameRegistrationTbox.Text,
                        Descriptionn = UserEmailRegistrationTbox.Text,
                        Price = decimal.Parse(PriceTbox.Text),
                        Height = HeightTbox.Text,
                        Weight = WeightTbox.Text,
                        QuantityInStock = int.Parse(QuantityTbox.Text),
                        
                    };

                    var errors = AdminToyController.GetToyValidationErrors(updatedToy);
                    if (errors.Count() > 0)
                    {
                        MessageBox.Show($"Некорректные данные:\n\n{string.Join("\n", errors)}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = await API.UpdateToy(_selectedToy.ArticulToy, updatedToy);

                    if (result != null)
                    {
                        MessageBox.Show("Товар успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении товара", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    var newToy = new Toy
                    {
                        ArticulToy = GenerateArticul(),
                        Title = UserNameRegistrationTbox.Text,
                        Descriptionn = UserEmailRegistrationTbox.Text,
                        Price = decimal.Parse(PriceTbox.Text),
                        Height = HeightTbox.Text,
                        Weight = WeightTbox.Text,
                        QuantityInStock = int.Parse(QuantityTbox.Text)
                    };

                    var result = await API.AddToy(newToy);

                    if (result != null)
                    {
                        MessageBox.Show("Товар успешно добавлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении товара", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                AddBearGrid.Visibility = Visibility.Hidden;
                LoadToys();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CrossBtn_Click(object sender, RoutedEventArgs e)
        {
            AddBearGrid.Visibility = Visibility.Hidden;
            ClearForm();
        }

        private string GenerateArticul()
        {
            using (var context = new StorepinkteddybearBdContext())
            {
                var lastToy = context.Toys.OrderByDescending(t => t.ArticulToy).FirstOrDefault();
                if (lastToy != null && lastToy.ArticulToy.StartsWith("PTB"))
                {
                    if (int.TryParse(lastToy.ArticulToy.Substring(3), out int lastNumber))
                    {
                        return $"PTB{lastNumber + 1:000}";
                    }
                }
                return "PTB001";
            }
        }

        private void LoadToyToForm(Toy toy)
        {
            UserNameRegistrationTbox.Text = toy.Title;
            UserEmailRegistrationTbox.Text = toy.Descriptionn;
            PriceTbox.Text = toy.Price.ToString();
            HeightTbox.Text = toy.Height;
            WeightTbox.Text = toy.Weight;
            QuantityTbox.Text = toy.QuantityInStock.ToString();

            string imagePath = $"/ElementsVisualization/Bears/{toy.Title}.png";
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "ElementsVisualization", "Bears", $"{toy.Title}.png");
            if (File.Exists(fullPath))
            {
                PathToImageTbox.Text = imagePath;
                _selectedImagePath = fullPath;
            }
            else
            {
                PathToImageTbox.Text = "Изображение не найдено";
                _selectedImagePath = "";
            }
        }

        private void ClearForm()
        {
            UserNameRegistrationTbox.Text = "";
            UserEmailRegistrationTbox.Text = "";
            PriceTbox.Text = "";
            HeightTbox.Text = "";
            WeightTbox.Text = "";
            QuantityTbox.Text = "";
            PathToImageTbox.Text = "";
            _selectedImagePath = "";
            _selectedToy = null;
            _isEditMode = false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                ToysItemsControl.ItemsSource = _allToys;
            else
            {
                var filteredToys = _allToys.Where(t =>
                    t.Title.Contains(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) ||
                    (t.Descriptionn != null && t.Descriptionn.Contains(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                ToysItemsControl.ItemsSource = filteredToys;
            }
        }

        private async void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.Tag == null) return;

            if (comboBox.Items.Count == 0) return;

            int orderId = (int)comboBox.Tag;
            string newStatus = comboBox.SelectedValue.ToString();

            var order = _allOrders.FirstOrDefault(o => o.IdOrder == orderId);
            if (order != null)
            {
                bool isValidTransition = (order.StatusOrder == "в обработке" && newStatus == "отгружен") ||
                                         (order.StatusOrder == "отгружен" && newStatus == "доставлен");

            }


            if (newStatus != null && newStatus.Contains(":"))
            {
                newStatus = newStatus.Split(':').Last().Trim();
            }


            try
            {
                var result = await API.UpdateOrderStatus(orderId, newStatus);

                if (result != null)
                {
                    MessageBox.Show($"Статус заказа №{orderId} изменен на: {newStatus}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadAllOrders(); 
                }
                else
                {
                    MessageBox.Show("Не удалось обновить статус заказа", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Toy toy)
            {
                _selectedToy = toy;
            }
        }

        private void PriceTbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

    }
}