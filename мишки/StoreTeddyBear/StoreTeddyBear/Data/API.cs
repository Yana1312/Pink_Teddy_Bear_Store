using Castle.Core.Resource;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using StoreTeddyBear.Models;
using System.Text;

namespace StoreTeddyBear.Data
{
    public static class API
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _baseUrl = "http://localhost:5298/api";

        public static async Task<Useransadmin?> Auth(string email, string password)
        {
            var authData = new { Email = email, Password = password };
            var content = new StringContent(JsonConvert.SerializeObject(authData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/User/Authorization?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Useransadmin>(responseJson);
            }
            return null;
        }

        public static async Task<Useransadmin?> Registration(string email, string password, string name)
        {
            Useransadmin registData = new Useransadmin
            {
                EmailUsers = email,
                NameUsers = name,
                PasswordHash = password,
                StatusUsersProfile = "активный",
                RoleUsers = "пользователь"
            };
            var content = new StringContent(JsonConvert.SerializeObject(registData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/User/Registration", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Useransadmin>(responseJson);
            }
            return null;
        }

        public static async Task<Order?> AddToCart(int customerId, string articulToy, int quantity, string shippingAddress = "")
        {
            try
            {
                var cartRequest = new CartRequest
                {
                    CustomerId = customerId,
                    ArticulToy = articulToy,
                    Quantity = quantity,
                    ShippingAddress = shippingAddress
                };

                var content = new StringContent(JsonConvert.SerializeObject(cartRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Cart/AddToCart", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Order>(responseJson);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка добавления в корзину: {errorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления в корзину: {ex.Message}");
                return null;
            }
        }

        public static async Task<CartResponse?> GetCart(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Cart/GetCart/{customerId}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CartResponse>(responseJson);
                }
                return new CartResponse { Items = new List<CartItem>(), TotalAmount = 0 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения корзины: {ex.Message}");
                return new CartResponse { Items = new List<CartItem>(), TotalAmount = 0 };
            }
        }

        public static async Task<bool> RemoveFromCart(int orderItemId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Cart/RemoveFromCart/{orderItemId}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка удаления из корзины: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления из корзины: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> UpdateQuantity(int orderItemId, int newQuantity)
        {
            try
            {
                var updateRequest = new UpdateQuantityRequest
                {
                    OrderItemId = orderItemId,
                    NewQuantity = newQuantity
                };

                var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Cart/UpdateQuantity", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка обновления количества: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления количества: {ex.Message}");
                return false;
            }
        }

        public static async Task<Order?> Checkout(int orderId, string shippingAddress)
        {
            try
            {
                var checkoutRequest = new CheckoutRequest
                {
                    ShippingAddress = shippingAddress
                };

                var content = new StringContent(JsonConvert.SerializeObject(checkoutRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Cart/Checkout/{orderId}", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Order>(responseJson);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка оформления заказа: {errorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка оформления заказа: {ex.Message}");
                return null;
            }
        }

        // === МЕТОДЫ ДЛЯ РАБОТЫ С ОТЗЫВАМИ ===

        public static async Task<Review?> AddReview(string articulToy, int customerId, sbyte rating, string comment)
        {
            try
            {
                var reviewRequest = new ReviewRequest
                {
                    ArticulToy = articulToy,
                    CustomerId = customerId,
                    Rating = rating,
                    Comment = comment
                };

                var content = new StringContent(JsonConvert.SerializeObject(reviewRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Review/Add", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Review>(responseJson);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка добавления отзыва: {errorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления отзыва: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<ReviewResponse>?> GetReviewsByProduct(string articulToy)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Review/GetByProduct/{Uri.EscapeDataString(articulToy)}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ReviewResponse>>(responseJson);
                }
                return new List<ReviewResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения отзывов по товару: {ex.Message}");
                return new List<ReviewResponse>();
            }
        }

        public static async Task<List<Review>?> GetReviewsByCustomer(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Review/GetByCustomer/{customerId}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Review>>(responseJson);
                }
                return new List<Review>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения отзывов пользователя: {ex.Message}");
                return new List<Review>();
            }
        }

        public static async Task<Review?> EditReview(int reviewId, sbyte rating, string comment)
        {
            try
            {
                var editRequest = new EditReviewRequest
                {
                    Rating = rating,
                    Comment = comment
                };

                var content = new StringContent(JsonConvert.SerializeObject(editRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Review/Edit/{reviewId}", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Review>(responseJson);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка редактирования отзыва: {errorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка редактирования отзыва: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> DeleteReview(int reviewId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Review/Delete/{reviewId}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка удаления отзыва: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления отзыва: {ex.Message}");
                return false;
            }
        }

        public static async Task<AverageRatingResponse?> GetAverageRating(string articulToy)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Review/AverageRating/{Uri.EscapeDataString(articulToy)}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AverageRatingResponse>(responseJson);
                }
                return new AverageRatingResponse { AverageRating = 0, ReviewCount = 0 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения среднего рейтинга: {ex.Message}");
                return new AverageRatingResponse { AverageRating = 0, ReviewCount = 0 };
            }
        }

        // Добавьте эти методы в класс API
        public static async Task<Toy?> AddToy(Toy toy)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(toy), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/AdminToy/Add", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Toy>(responseJson);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка добавления товара: {errorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления товара: {ex.Message}");
                return null;
            }
        }

        public static async Task<Toy?> UpdateToy(string articulToy, Toy updatedToy)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(updatedToy), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/AdminToy/Update/{Uri.EscapeDataString(articulToy)}", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Toy>(responseJson);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка обновления товара: {errorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления товара: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> DeleteToy(string articulToy)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/AdminToy/Delete/{Uri.EscapeDataString(articulToy)}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка удаления товара: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления товара: {ex.Message}");
                return false;
            }
        }

        public static async Task<List<Toy>?> SearchToys(string searchTerm)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/AdminToy/Search?searchTerm={Uri.EscapeDataString(searchTerm)}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Toy>>(responseJson);
                }
                return new List<Toy>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка поиска товаров: {ex.Message}");
                return new List<Toy>();
            }
        }

        public class CartRequest
        {
            public int CustomerId { get; set; }
            public string ArticulToy { get; set; }
            public int Quantity { get; set; }
            public string ShippingAddress { get; set; }
        }

        public class UpdateQuantityRequest
        {
            public int OrderItemId { get; set; }
            public int NewQuantity { get; set; }
        }

        public class CheckoutRequest
        {
            public string ShippingAddress { get; set; }
        }

        public class CartResponse
        {
            public int OrderId { get; set; }
            public List<CartItem> Items { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class CartItem
        {
            public int OrderItemId { get; set; }
            public string ArticulToy { get; set; }
            public string Title { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }

        // === КЛАССЫ ДЛЯ РАБОТЫ С ОТЗЫВАМИ ===

        public class ReviewRequest
        {
            public string ArticulToy { get; set; }
            public int CustomerId { get; set; }
            public sbyte Rating { get; set; }
            public string Comment { get; set; }
        }

        public class EditReviewRequest
        {
            public sbyte Rating { get; set; }
            public string Comment { get; set; }
        }

        public class ReviewResponse
        {
            public int IdReview { get; set; }
            public string CustomerName { get; set; }
            public sbyte Rating { get; set; }
            public string Comment { get; set; }
            public DateTime? Date { get; set; }
        }

        public class AverageRatingResponse
        {
            public decimal AverageRating { get; set; }
            public int ReviewCount { get; set; }
        }
    }
}