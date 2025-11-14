using Castle.Core.Resource;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using StoreTeddyBear.Models;
using System.Text;
using System.Xml.Linq;

namespace StoreTeddyBear.Data
{
    public static class API
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _baseUrl = "http://localhost:5298/api";

        public static async Task<Useransadmin?> Auth(string email, string password)
        {
            var authData = new { email, password };
            var content = new StringContent(JsonConvert.SerializeObject(authData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/User/Authorization", content);

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

        public static async Task<Useransadmin?> EditProfile(int id,string email, string password, string name)
        {
            Useransadmin updateData = new Useransadmin
            {
                EmailUsers = email,
                NameUsers = name,
                PasswordHash = password,
                StatusUsersProfile = "активный",
                RoleUsers = "пользователь"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updateData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_baseUrl}/User/{id}/EditProfile", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Useransadmin>(responseJson);
            }
            return null;
        }

        public static async Task<Useransadmin?> DeactivateCustomer(int id)
        {
            var user = StorepinkteddybearBdContext.Instance.Useransadmins.Find(id);
            Useransadmin updateData = new Useransadmin
            {
                EmailUsers = user.EmailUsers,
                NameUsers = user.NameUsers,
                PasswordHash = user.PasswordHash,
                StatusUsersProfile = "неактивный",
                RoleUsers = "пользователь"
            };
            var response = await _httpClient.PatchAsync($"{_baseUrl}/User/{id}/Deactivate", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Useransadmin>(responseJson);
            }
            return null;
        }

        public static async Task<Order?> AddToCart(int customerId, string articulToy, int quantity)
        {
            try
            {
                var cart = new
                {
                    customerId,
                    articulToy,
                    quantity
                };

                var content = new StringContent(JsonConvert.SerializeObject(cart), Encoding.UTF8, "application/json");
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

        public static async Task<Order?> GetCart(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Cart/GetCart/{customerId}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Order>(responseJson);
                }
                return new Order { Orderitems = new List<Orderitem>(), TotalAmount = 0 };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения корзины: {ex.Message}");
                return new Order { Orderitems = new List<Orderitem>(), TotalAmount = 0 };
            }
        }

        public static async Task<bool> RemoveFromCart(int orderItemId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Cart/RemoveFromCart/{orderItemId}");

                if (response.IsSuccessStatusCode)  return true;
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
                var updateQuantity = new 
                {
                    orderItemId,
                    newQuantity
                };

                var content = new StringContent(JsonConvert.SerializeObject(updateQuantity), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Cart/UpdateQuantity", content);

                if (response.IsSuccessStatusCode) return true;
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
                var content = new StringContent(JsonConvert.SerializeObject(shippingAddress), Encoding.UTF8, "application/json");
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


        public static async Task<Review?> AddReview(string articulToy, int customerId, sbyte rating, string comment)
        {
            try
            {
                var review = new Review
                {
                    ArticulToy = articulToy,
                    IdCustomer = customerId,
                    RatingReview = rating,
                    CommentReview = comment
                };

                var content = new StringContent(JsonConvert.SerializeObject(review), Encoding.UTF8, "application/json");
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

        public static async Task<List<Review>?> GetReviewsByProduct(string articulToy)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Review/GetByProduct/{Uri.EscapeDataString(articulToy)}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Review>>(responseJson);
                }
                return new List<Review>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения отзывов по товару: {ex.Message}");
                return new List<Review>();
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
                var editRequest = new
                {
                    rating,
                    comment
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

                if (response.IsSuccessStatusCode)  return true;
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

                if (response.IsSuccessStatusCode) return true;
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

        public static async Task<List<Order>> GetCustomerOrders(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/OrderController/{customerId}/CustomerOrders");
                if (response.IsSuccessStatusCode) 
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Order>>(responseJson);
                }
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения заказов покупателя: {ex.Message}");
                return new List<Order>();
            }
        }

        public static async Task<List<Order>> GetOrderDetails(int orderId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/OrderController/{orderId}/Details");
                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Order>>(responseJson);
                }
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения деталей заказа: {ex.Message}");
                return new List<Order>();
            }
        }

        public static async Task<Order> UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                var statusOrder = new
                {
                    orderId,
                    newStatus
                };
                var content = new StringContent(JsonConvert.SerializeObject(statusOrder), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/OrderController/{orderId}/UpdateStatus",  content);
                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Order>(responseJson);
                }
                return new Order();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления статуса заказа: {ex.Message}");
                return new Order();
            }
        }



        public class AverageRatingResponse
        {
            public decimal AverageRating { get; set; }
            public int ReviewCount { get; set; }
        }
    }
}