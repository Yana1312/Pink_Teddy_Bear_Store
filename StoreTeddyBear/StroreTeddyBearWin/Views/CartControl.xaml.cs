using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq;

namespace StroreTeddyBearWin.Views
{
    public partial class CartControl : UserControl
    {
        private Orderitem _cartItem;

        public CartControl(Orderitem cartItem)
        {
            InitializeComponent();
            _cartItem = cartItem;
            DataContext = _cartItem;
            LoadToyData();
        }

        public event Action<CartControl> ItemRemoved;
        public event Action<CartControl> QuantityUpdated;

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

        private void LoadToyData()
        {
            try
            {
                var toy = StorepinkteddybearBdContext.Instance.Toys.
                    FirstOrDefault(t => t.ArticulToy == _cartItem.ArticulToy);
                if (toy != null)
                {
                    TitleBearTb.Text = toy.Title;
                    DescriptionBearTb.Text = toy.Descriptionn;
                    WeightAndHeightBearTb.Text = $"высота: {toy.Height}, вес: {toy.Weight}";
                    PriceTb.Text = $"{toy.Price:F2} ₽";
                    CountTb.Text = _cartItem.Quantity.ToString();
                    LoadImage(BearInItemsCartImg, $"/ElementsVisualization/Bears/{toy.Title}.png");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddcountBtn_Click(object sender, RoutedEventArgs e)
        {
            await UpdateQuantity(int.Parse(CountTb.Text) + 1);
        }

        private async void DiscountBtn_Click(object sender, RoutedEventArgs e)
        {
            await UpdateQuantity(int.Parse(CountTb.Text) - 1);
        }

        private async void RemoveFromCart()
        {
            try
            {
                var res = await API.RemoveFromCart(_cartItem.IdOrderItem);
                if (res)
                {
                    ItemRemoved?.Invoke(this);
                    MessageBox.Show("Товар удален из корзины", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateQuantity(int newQuantity)
        {
            try
            {
                var toy = StorepinkteddybearBdContext.Instance.Toys.
                    FirstOrDefault(t => t.ArticulToy == _cartItem.ArticulToy);

                if (newQuantity <= 0)
                {
                    RemoveFromCart();
                    return;
                }

                if (toy.QuantityInStock < newQuantity)
                {
                    MessageBox.Show($"Недостаточно товара на складе. Доступно: {toy.QuantityInStock}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var orderItem = await API.UpdateQuantity(_cartItem.IdOrderItem, newQuantity);
                if (orderItem == null) throw new Exception();

                CountTb.Text = orderItem.Quantity.ToString();

                QuantityUpdated?.Invoke(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления количества товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Basket_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RemoveFromCart();
        }
    }
}