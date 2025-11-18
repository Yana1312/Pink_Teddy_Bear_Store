using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StroreTeddyBearWin.Views
{
    public partial class CartControl : UserControl
    {
        private Orderitem _cartItem;

        public CartControl(Orderitem cartItem)
        {
            _cartItem = cartItem;
            InitializeComponent();
            DataContext = _cartItem;
            LoadToyData();
        }

        private void LoadToyData()
        {
            try
            {
                var toy = StorepinkteddybearBdContext.Instance.Toys.FirstOrDefault(t => t.ArticulToy == _cartItem.ArticulToy);
                if (toy != null)
                {
                    TitleBearTb.Text = toy.Title;
                    DescriptionBearTb.Text = toy.Descriptionn;
                    WeightAndHeightBearTb.Text = $"высота: {toy.Height}, вес: {toy.Weight}";
                    PriceTb.Text = $"{toy.Price:F2} ₽";
                    CountTb.Text = _cartItem.Quantity.ToString();

                    try
                    {
                        BearInItemsCartImg.Source = new BitmapImage(
                            new Uri($"/ElementsVisualization/Bears/{toy.Title}.png", UriKind.Relative));
                    }
                    catch
                    {
                        BearInItemsCartImg.Source = new BitmapImage(
                            new Uri("/ElementsVisualization/Image/placeholder.png", UriKind.Relative));
                    }
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
            if (CountTb.Text == "0") RemoveFromCart();
        }

        private async void DiscountBtn_Click(object sender, RoutedEventArgs e)
        {
           await UpdateQuantity(int.Parse(CountTb.Text) - 1);
            if (CountTb.Text == "0") RemoveFromCart();
        }

        private async void RemoveFromCart()
        {
            try
            {
                var res = await API.RemoveFromCart(_cartItem.IdOrderItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка  удаления товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateQuantity(int newQuantity)
        {
            try
            {
                var quantity = await API.UpdateQuantity(_cartItem.IdOrderItem, newQuantity);
                CountTb.Text = quantity.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления количества товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}