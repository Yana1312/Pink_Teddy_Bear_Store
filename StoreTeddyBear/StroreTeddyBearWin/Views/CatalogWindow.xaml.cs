using StoreTeddyBear.Models;
using StoreTeddyBear.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace StroreTeddyBearWin.Views
{
    public partial class CatalogWindow : Window
    {
        private List<Toy> _allToys = StorepinkteddybearBdContext.Instance.Toys.ToList();
        private int _currentToyIndex = 0;
        private List<string> _imagePaths = new List<string>();
        private Useransadmin _currentUser;

        public CatalogWindow(Useransadmin user)
        {
            InitializeComponent();

            _currentUser = user;

            LoadToys();
            DisplayCurrentToy();
            UpdateAddToCartButtonState();
        }

        private void LoadToys()
        {
            try
            {
                foreach (var item in _allToys)
                    _imagePaths.Add($"/ElementsVisualization/Bears/{item.Title}.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки игрушек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayCurrentToy()
        {
            if (_allToys == null || !_allToys.Any()) return;

            var currentToy = _allToys[_currentToyIndex];

            TitleBearTbox.Text = currentToy.Title;
            DescriptionBearTbox.Text = currentToy.Descriptionn;

            LoadImage(CurrentBearImg, _imagePaths[_currentToyIndex]);
            UpdateSideImages();
        }

        private void UpdateSideImages()
        {
            int leftIndex = GetPreviousIndex();
            int rightIndex = GetNextIndex();

            LoadImage(LeftBearImg, _imagePaths[leftIndex]);
            LoadImage(RightBearImg, _imagePaths[rightIndex]);
        }

        private int GetPreviousIndex()
        {
            int leftIndex = _currentToyIndex - 1;
            return leftIndex < 0 ? _allToys.Count - 1 : leftIndex;
        }

        private int GetNextIndex()
        {
            int rightIndex = _currentToyIndex + 1;
            return rightIndex >= _allToys.Count ? 0 : rightIndex;
        }

        private void LoadImage(System.Windows.Controls.Image image, string imagePath)
        {

            try
            {
                if (File.Exists(@"C:\Users\user\Desktop\проекты\проекты WPF\GitHub Bears\StoreTeddyBear\StroreTeddyBearWin\" + imagePath))
                    image.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                else throw new Exception();
            }
            catch (Exception)
            {
                image.Source = new BitmapImage(new Uri("/ElementsVisualization/Image/placeholder.png", UriKind.Relative));
            }
        }

        private void UpdateAddToCartButtonState()
        {
            if (_currentUser != null && _allToys != null && _allToys.Any())
            {
                AddToCartButton.IsEnabled = true;
                AddToCartButton.Content = "Добавить в корзину";
            }
            else
            {
                AddToCartButton.IsEnabled = false;
                AddToCartButton.Visibility = Visibility.Hidden;
            }
        }

        private async void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allToys == null || !_allToys.Any()) return;

            if (_currentUser == null)
            {
                MessageBox.Show("Для добавления товаров в корзину необходимо авторизоваться",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentToy = _allToys[_currentToyIndex];

            try
            {
                AddToCartButton.IsEnabled = false;
                AddToCartButton.Content = "Добавление...";

                var result = await API.AddToCart(
                    _currentUser.IdCustomer,
                    currentToy.ArticulToy,
                    1
                );

                if (result != null)
                {
                    MessageBox.Show($"Мишка '{currentToy.Title}' добавлен в корзину!",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось добавить товар в корзину. Возможно, товара нет в наличии.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateAddToCartButtonState();
            }
        }

        private void RightArrowsImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_allToys == null || !_allToys.Any()) return;

            _currentToyIndex = GetNextIndex();
            DisplayCurrentToy();
        }

        private void LeftArrowsImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_allToys == null || !_allToys.Any()) return;

            _currentToyIndex = GetPreviousIndex();
            DisplayCurrentToy();
        }

        private void LeftBearImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LeftArrowsImg_MouseDown(sender, e);
        }

        private void RightBearImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RightArrowsImg_MouseDown(sender, e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Left)
                LeftArrowsImg_MouseDown(null, null);
            else if (e.Key == Key.Right)
                RightArrowsImg_MouseDown(null, null);
            else if (e.Key == Key.Escape)
                BackToMainWindowImg_MouseDown(null, null);
            else if (e.Key == Key.Enter || e.Key == Key.Add)
                AddToCartButton_Click(null, null);
        }

        private void BackToMainWindowImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }

        private void CartBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Войдите, чтобы иметь возможность просмотра корзины","Ошибка", MessageBoxButton.OK ,MessageBoxImage.Warning);
                return;
            }
            CartCatalog cartCatalog = new CartCatalog(_currentUser);
            cartCatalog.Show();
            this.Close();
        }
    }
}