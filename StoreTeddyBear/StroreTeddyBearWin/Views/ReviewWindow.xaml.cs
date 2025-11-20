using Castle.Components.DictionaryAdapter.Xml;
using StoreTeddyBear.Controllers;
using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static MaterialDesignThemes.Wpf.Theme;

namespace StroreTeddyBearWin.Views
{
    /// <summary>
    /// Логика взаимодействия для ReviewWindow.xaml
    /// </summary>
    public partial class ReviewWindow : Window
    {
        private Toy Toy;
        private Useransadmin currentUser;
        public ReviewWindow(Toy toy, Useransadmin user)
        {
            InitializeComponent();
            Toy = toy;
            currentUser = user;
            LoadToy();
            LoadReviewsByToy();
            if (currentUser == null)
            {
                ReviewBtn.Visibility = Visibility.Hidden;
                DeleteReviewBtn.Visibility = Visibility.Hidden;
                EditReviewBtn.Visibility = Visibility.Hidden;
            }
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


        private async Task LoadReviewsByToy()
        {
            var res = await API.GetReviewsByProduct(Toy.ArticulToy);
            if (res == null) 
            {
                MessageBox.Show("Отзывы на данного мишку отсутствуют...", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ReviewsByToyLv.ItemsSource = res;
            LoadImage(BearInItemsCartImg, $"/ElementsVisualization/Bears/{Toy.Title}.png");
        }

        private async Task LoadToy()
        {
            DataContext = Toy;

            Weighttb.Text = "Вес:" + Toy.Weight;
            HeightTb.Text = "Рост: " + Toy.Height;
            PriceTb.Text = "Цена: " + Toy.Price.ToString();

            var res = await API.GetAverageRating(Toy.ArticulToy);
            
            if (res == null)
            {
                MessageBox.Show("Средний рейтинг не найден. Возможная причина: отсутствие отзывов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                RatingAvgTb.Text = $"Средний рейтинг: {res.AverageRating.ToString()}";
            }
        }

        private void CartBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Войдите, чтобы иметь возможность просмотра корзины", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CartCatalog cartCatalog = new CartCatalog(currentUser);
            cartCatalog.Show();
            this.Close();
        }

        private void BackToMainWindowImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentUser == null)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                CatalogWindow catalogWindow = new CatalogWindow(currentUser);
                catalogWindow.Show();
                this.Close();
            }
        }

        private void NextToyBtn_Click(object sender, RoutedEventArgs e)
        {
            int currentToyIndex = toys.FindIndex(t => t.ArticulToy == Toy.ArticulToy);
            if (currentToyIndex == -1) currentToyIndex = 0;
            Toy = toys[GetNextIndex(currentToyIndex)];
            DataContext = Toy;
            LoadToy();
            LoadReviewsByToy();
        }
         public List<Toy> toys = StorepinkteddybearBdContext.Instance.Toys.ToList();

        private int GetNextIndex(int _currentToyIndex)
        {
            int rightIndex = _currentToyIndex + 1;
            return rightIndex >= toys.Count ? 0 : rightIndex;
        }

        private async void EditReview()
        {
            try
            {
               
                AddReviewBtn.IsEnabled = false;
                AddReviewBtn.Content = "      Редактирование...     ";
                int rating = int.Parse(RatingTbox.Text);
                var errors = ReviewController.GetValidationErrors(rating, ReviewAddTbox.Text);
                if (errors.Count > 0)
                {
                    MessageBox.Show($"Некорректные данные:\n\n{string.Join("\n", errors)}");
                    return;
                }
                var res = await API.EditReview(selectedReview.IdReview, (sbyte)rating, ReviewAddTbox.Text);

                if (res != null)
                {
                    MessageBox.Show("Отзыв изменился!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadReviewsByToy();
                }
                else MessageBox.Show($"Не удалось отредактировать отзыв.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования отзыва: {ex.Message}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AddReviewBtn.IsEnabled = true;
                AddReviewBtn.Content = "      Отредактировать     ";
                AddReviewGrid.Visibility = Visibility.Hidden;
                LoadToy();
            }
        }

        private async void AddReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mainTextTb.Text == "Редактирование отзыва")
            {
                EditReview();
                return;
            }
            try
            {
                AddReviewBtn.IsEnabled = false;
                AddReviewBtn.Content = "      Добавление...     ";
                int rating = int.Parse(RatingTbox.Text);
                var errors = ReviewController.GetValidationErrors(rating, ReviewAddTbox.Text);
                if (errors.Count > 0)
                {
                    MessageBox.Show($"Некорректные данные:\n\n{string.Join("\n", errors)}");
                    return;
                }
                var res = await API.AddReview(Toy.ArticulToy, currentUser.IdCustomer, (sbyte)rating, ReviewAddTbox.Text);

                if (res != null)
                {
                    MessageBox.Show("Отзыв добавился!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadReviewsByToy();
                }
                else MessageBox.Show($"Не удалось добавить отзыв.{res}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления отзыва: {ex.Message}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AddReviewBtn.IsEnabled = true;
                AddReviewBtn.Content = "      Добавить отзыв     ";
            }
        }


        private void CrossBtn_Click(object sender, RoutedEventArgs e)
        {
            AddReviewGrid.Visibility = Visibility.Hidden;
            this.Title = "Окно отзывов";
        }

        private void ReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            AddReviewGrid.Visibility = Visibility.Visible;
            this.Title = "Добавление отзыва";
        }

        private void RatingTbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) || e.Text[0] < '1' || e.Text[0] > '5')
            {
                e.Handled = true;
            }
        }


        private async void DeleteReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedReview == null)
            {
                MessageBox.Show("Выберите отзыв для удаления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedReview.IdCustomer != currentUser.IdCustomer)
            {
                MessageBox.Show("Вы можете удалять только свои отзывы", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show($"Вы уверены, что хотите удалить отзыв '{selectedReview.CommentReview}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await API.DeleteReview(selectedReview.IdReview);

                    if (success)
                    {
                        MessageBox.Show("Отзыв успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadReviewsByToy();
                        ReviewsByToyLv.SelectedItem = null;
                        selectedReview = null;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить отзыв.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении отзыва: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditReviewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedReview == null)
            {
                MessageBox.Show("Выберите отзыв для редактирования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedReview.IdCustomer != currentUser.IdCustomer)
            {
                MessageBox.Show("Вы можете редактировать только свои отзывы", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RatingTbox.Text = selectedReview.RatingReview.ToString();
            ReviewAddTbox.Text = selectedReview.CommentReview.ToString();

            AddReviewBtn.Content = "Отредактировать";
            mainTextTb.Text = "Редактирование отзыва";
            AddReviewGrid.Visibility = Visibility.Visible;
            this.Title = "Редактирование отзыва";

        }
        public Review selectedReview;

        private void ReviewsByToyLv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedReview = ReviewsByToyLv.SelectedItem as Review;
        }
    }
}
