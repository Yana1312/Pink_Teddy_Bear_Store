using Microsoft.Win32;
using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace StroreTeddyBearWin.Views
{
    public partial class AdminWindow : Window
    {
        private List<Toy> _allToys;
        private Toy _selectedToy;
        private bool _isEditMode = false;
        private string _selectedImagePath = "";

        public AdminWindow()
        {
            InitializeComponent();
            LoadToys();
        }

        private async void LoadToys()
        {
            try
            {
                using (var context = new StorepinkteddybearBdContext())
                {
                    _allToys = context.Toys.ToList();
                    BearsItemsLv.ItemsSource = _allToys;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    // Используем API для удаления
                    var success = await API.DeleteToy(_selectedToy.ArticulToy);

                    if (success)
                    {
                        MessageBox.Show("Товар успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadToys();
                        ClearSelection();
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
            CrossBtn.ToolTip = "Закрыть форму добавления";
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
            CrossBtn.ToolTip = "Закрыть форму редактирования";
        }

        private void GetPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                PathToImageTbox.Text = _selectedImagePath;

                // Копируем изображение в папку проекта
                try
                {
                    CopyImageToProject(_selectedImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка копирования изображения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void CopyImageToProject(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                return;

            try
            {
                var fileName = Path.GetFileName(sourcePath);
                var projectImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "ElementsVisualization", "Bears", fileName);

                // Создаем директорию если не существует
                var directory = Path.GetDirectoryName(projectImagePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Копируем файл
                File.Copy(sourcePath, projectImagePath, true);

                MessageBox.Show($"Изображение скопировано: {projectImagePath}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось скопировать изображение: {ex.Message}");
            }
        }

        private async void SetUpBearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                if (_isEditMode)
                {
                    // Редактирование существующего товара через API
                    var updatedToy = new Toy
                    {
                        ArticulToy = _selectedToy.ArticulToy,
                        Title = UserNameRegistrationTbox.Text,
                        Descriptionn = UserEmailRegistrationTbox.Text,
                        Price = decimal.Parse(PriceTbox.Text),
                        Height = HeightTbox.Text,
                        Weight = WeightTbox.Text,
                        QuantityInStock = int.Parse(QuantityTbox.Text)
                    };

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
                    // Добавление нового товара через API
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

        private void BearsItemsLv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedToy = BearsItemsLv.SelectedItem as Toy;
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

            PathToImageTbox.Text = $"/ElementsVisualization/Bears/{toy.Title}.png";
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(UserNameRegistrationTbox.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(UserEmailRegistrationTbox.Text))
            {
                MessageBox.Show("Введите описание товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(PriceTbox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(HeightTbox.Text))
            {
                MessageBox.Show("Введите рост товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(WeightTbox.Text))
            {
                MessageBox.Show("Введите вес товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(QuantityTbox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
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

        private void ClearSelection()
        {
            BearsItemsLv.SelectedItem = null;
            _selectedToy = null;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allToys == null) return;
            var searchTerm = (sender as TextBox)?.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                BearsItemsLv.ItemsSource = _allToys;
            }
            else
            {
                var filteredToys = _allToys.Where(t =>
                    t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Descriptionn.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.ArticulToy.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                BearsItemsLv.ItemsSource = filteredToys;
            }
        }
    }
}