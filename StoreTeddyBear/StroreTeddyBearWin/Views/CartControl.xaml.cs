using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StroreTeddyBearWin.Views
{
    public partial class CartControl : UserControl
    {
        public event EventHandler<int> RemoveItemClicked;
        public event EventHandler<int> IncreaseQuantityClicked;
        public event EventHandler<int> DecreaseQuantityClicked;

        private Orderitem _cartItem;

        public CartControl(Orderitem cartItem)
        {
            _cartItem = cartItem;
            InitializeComponent();
            LoadToyData();
        }

        private void LoadToyData()
        {
            try
            {
                // Загружаем полную информацию о товаре из базы данных
                using (var context = new StorepinkteddybearBdContext())
                {
                    var toy = context.Toys.FirstOrDefault(t => t.ArticulToy == _cartItem.ArticulToy);
                    if (toy != null)
                    {
                        // Устанавливаем данные
                        TitleBearTb.Text = toy.Title;
                        DescriptionBearTb.Text = toy.Descriptionn;
                        WeightAndHeightBearTb.Text = $"высота: {toy.Height}, вес: {toy.Weight}";
                        PriceTb.Text = $"{toy.Price:F2} ₽";
                        CountTb.Text = _cartItem.Quantity.ToString();

                        // Загружаем изображение
                        try
                        {
                            BearInItemsCartImg.Source = new BitmapImage(
                                new Uri($"/ElementsVisualization/Bears/{toy.Title}.png", UriKind.Relative));
                        }
                        catch
                        {
                            // Используем изображение-заглушку при ошибке
                            BearInItemsCartImg.Source = new BitmapImage(
                                new Uri("/ElementsVisualization/Image/placeholder.png", UriKind.Relative));
                        }
                    }
                }

                // Устанавливаем Tag кнопок для идентификации товара
                AddcountBtn.Tag = _cartItem.IdOrderItem;
                DiscountBtn.Tag = _cartItem.IdOrderItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddcountBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AddcountBtn.Tag is int orderItemId)
            {
                IncreaseQuantityClicked?.Invoke(this, orderItemId);
            }
        }

        private void DiscountBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DiscountBtn.Tag is int orderItemId)
            {
                DecreaseQuantityClicked?.Invoke(this, orderItemId);
            }
        }

        // Метод для обновления количества (вызывается из CartCatalog после успешного обновления)
        public void UpdateQuantity(int newQuantity)
        {
            CountTb.Text = newQuantity.ToString();
        }
    }
}