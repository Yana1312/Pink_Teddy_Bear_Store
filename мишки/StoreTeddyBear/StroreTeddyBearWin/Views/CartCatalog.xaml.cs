using StoreTeddyBear.Data;
using StoreTeddyBear.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StroreTeddyBearWin.Views
{
    public partial class CartCatalog : Window
    {
        private Useransadmin _currentUser;
        private API.CartResponse _currentCart;

        public CartCatalog(Useransadmin useransadmin)
        {
            InitializeComponent();
            _currentUser = useransadmin;
            LoadAddresses();
            LoadCart();
        }

        private void LoadAddresses()
        {
            try
            {
                List<string> addresses = new List<string>
                {
                    "г. Москва, ул. Тверская, д. 25, кв. 14",
                    "г. Санкт-Петербург, Невский пр-т, д. 100, кв. 32",
                    "г. Казань, ул. Баумана, д. 45, кв. 7"
                };
                AdressCbox.ItemsSource = addresses;
                if (addresses.Any())
                    AdressCbox.SelectedItem = addresses.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки адресов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadCart()
        {
            try
            {
                _currentCart = await API.GetCart(_currentUser.IdCustomer);

                if (_currentCart != null && _currentCart.Items != null && _currentCart.Items.Any())
                {
                    CartItemsLv.Items.Clear();

                    foreach (var cartItem in _currentCart.Items)
                    {
                        // Создаем пользовательский элемент управления для каждого товара в корзине
                        var cartControl = new CartControl(cartItem);
                        cartControl.RemoveItemClicked += OnRemoveItemClicked;
                        cartControl.IncreaseQuantityClicked += OnIncreaseQuantityClicked;
                        cartControl.DecreaseQuantityClicked += OnDecreaseQuantityClicked;

                        CartItemsLv.Items.Add(cartControl);
                    }

                    UpdateTotalAmount();
                    CreateOrderBtn.IsEnabled = true;
                    ClearCartBtn.IsEnabled = true;
                }
                else
                {
                    CartItemsLv.Items.Clear();
                    CartItemsLv.Items.Add(new TextBlock
                    {
                        Text = "Корзина пуста",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 16
                    });
                    TotalAmountText.Text = "Итоговая сумма заказа: 0 рублей";
                    CreateOrderBtn.IsEnabled = false;
                    ClearCartBtn.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotalAmount()
        {
            if (_currentCart != null)
            {
                TotalAmountText.Text = $"Итоговая сумма заказа: {_currentCart.TotalAmount:F2} рублей";
            }
            else
            {
                TotalAmountText.Text = "Итоговая сумма заказа: 0 рублей";
            }
        }

        // Обработчики событий из CartControl
        private async void OnRemoveItemClicked(object sender, int orderItemId)
        {
            await RemoveCartItem(orderItemId);
        }

        private async void OnIncreaseQuantityClicked(object sender, int orderItemId)
        {
            await UpdateCartItemQuantity(orderItemId, true);
        }

        private async void OnDecreaseQuantityClicked(object sender, int orderItemId)
        {
            await UpdateCartItemQuantity(orderItemId, false);
        }

        private async System.Threading.Tasks.Task RemoveCartItem(int orderItemId)
        {
            try
            {
                var result = await API.RemoveFromCart(orderItemId);
                if (result)
                {
                    MessageBox.Show("Товар удален из корзины", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadCart();
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task UpdateCartItemQuantity(int orderItemId, bool increase)
        {
            try
            {
                var currentItem = _currentCart.Items.FirstOrDefault(i => i.OrderItemId == orderItemId);
                if (currentItem != null)
                {
                    int newQuantity = increase ? currentItem.Quantity + 1 : currentItem.Quantity - 1;

                    if (newQuantity <= 0)
                    {
                        await RemoveCartItem(orderItemId);
                        return;
                    }

                    var result = await API.UpdateQuantity(orderItemId, newQuantity);
                    if (result)
                    {
                        LoadCart();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить количество товара", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении количества: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentCart == null || !_currentCart.Items.Any())
                {
                    MessageBox.Show("Корзина пуста!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedAddress = AdressCbox.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(selectedAddress))
                {
                    MessageBox.Show("Выберите адрес доставки!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = await API.Checkout(_currentCart.OrderId, selectedAddress);

                if (result != null)
                {
                    MessageBox.Show($"Заказ №{result.IdOrder} успешно оформлен!\nСтатус: {result.StatusOrder}",
                        "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                    CreateOrderBtn.IsEnabled = false;
                    ClearCartBtn.IsEnabled = false;

                    _currentCart = null;
                    CartItemsLv.Items.Clear();
                    TotalAmountText.Text = "Заказ успешно оформлен!";
                }
                else
                {
                    MessageBox.Show("Ошибка при оформлении заказа. Проверьте наличие товаров на складе.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClearCartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentCart == null || !_currentCart.Items.Any())
                {
                    MessageBox.Show("Корзина уже пуста!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show("Вы уверены, что хотите очистить всю корзину?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var item in _currentCart.Items.ToList())
                    {
                        await API.RemoveFromCart(item.OrderItemId);
                    }

                    MessageBox.Show("Корзина очищена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadCart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при очистке корзины: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMainWindowImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CatalogWindow catalogWindow = new CatalogWindow(_currentUser);
            catalogWindow.Show();
            this.Close();
        }
    }
}