using Castle.Core.Resource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreTeddyBear.Models;
using System.Data;

namespace StoreTeddyBear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Добавление товара в корзину
    // Получение корзины
    // Удаление товара из корзины
    // Обновление количества товара в корзине
    // Оформление заказа

    public class CartController : ControllerBase
    {
        [HttpPost("AddToCart")]
        public ActionResult<Order> AddToCart(int CustomerId, string ArticulToy, int Quantity)
        {
            var customer = StorepinkteddybearBdContext.Instance.Useransadmins
                .FirstOrDefault(c => c.IdCustomer == CustomerId && c.StatusUsersProfile == "активный");

            if (customer == null)
                return NotFound("Пользователь не найден или аккаунт неактивен");

            var toy = StorepinkteddybearBdContext.Instance.Toys
                .FirstOrDefault(t => t.ArticulToy == ArticulToy);

            if (toy == null)
                return NotFound("Товар не найден");

            if (toy.QuantityInStock < Quantity)
                return BadRequest($"Недостаточно товара на складе. Доступно: {toy.QuantityInStock}");
            //1 этап - ожидает подтверждения
            //2 этап - в обработке
            //3 этап - отгружен
            //4 этап - доставлен
            //отгружен — завершающий статус, а в обработке — этап, предшествующий отгрузке
            var existingOrder = StorepinkteddybearBdContext.Instance.Orders
                .Include(o => o.Orderitems)
                .FirstOrDefault(o => o.IdCustomer == CustomerId && o.StatusOrder == "ожидает подтверждения");

            Order order;

            if (existingOrder == null)
            {
                order = new Order
                {
                    IdCustomer = CustomerId,
                    StatusOrder = "ожидает подтверждения",
                    AdressOrder = "",
                    DateOrder = DateTime.Now,
                    TotalAmount = 0
                };
                StorepinkteddybearBdContext.Instance.Orders.Add(order);
                StorepinkteddybearBdContext.Instance.SaveChanges();
            }
            else
            {
                order = existingOrder;
            }

            var existingOrderItem = StorepinkteddybearBdContext.Instance.Orderitems
                .FirstOrDefault(oi => oi.IdOrder == order.IdOrder && oi.ArticulToy == ArticulToy);

            if (existingOrderItem != null)
            {
                existingOrderItem.Quantity += Quantity;
            }
            else
            {
                var orderItem = new Orderitem
                {
                    IdOrder = order.IdOrder,
                    ArticulToy = ArticulToy,
                    Quantity = Quantity,
                    UnitPrice = toy.Price
                };
                StorepinkteddybearBdContext.Instance.Orderitems.Add(orderItem);
            }

            UpdateOrderTotalAmount(order.IdOrder);

            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(order);
        }

        [HttpGet("{customerId}/GetCart")]
        public ActionResult<Order> GetCart(int customerId)
        {
            var order = StorepinkteddybearBdContext.Instance.Orders
                .Include(o => o.Orderitems)
                .FirstOrDefault(o => o.IdCustomer == customerId && o.StatusOrder == "ожидает подтверждения");
            if (order == null)
            {
                order = new Order
                {
                    IdCustomer = customerId,
                    StatusOrder = "ожидает подтверждения",
                    AdressOrder = "",
                    DateOrder = DateTime.Now,
                    TotalAmount = 0
                };
            }
            UpdateOrderTotalAmount(order.IdOrder);
            return Ok(order);
        }

        [HttpDelete("{orderItemId}/RemoveFromCart")]
        public ActionResult RemoveFromCart(int orderItemId)
        {
            var orderItem = StorepinkteddybearBdContext.Instance.Orderitems.Find(orderItemId);

            if (orderItem == null)
                return NotFound("Товар в корзине не найден");

            var orderId = orderItem.IdOrder;
            StorepinkteddybearBdContext.Instance.Orderitems.Remove(orderItem);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            UpdateOrderTotalAmount(orderId);

            var remainingItems = StorepinkteddybearBdContext.Instance.Orderitems
                .Count(oi => oi.IdOrder == orderId);

            if (remainingItems == 0)
            {
                var order = StorepinkteddybearBdContext.Instance.Orders.Find(orderId);
                if (order != null)
                {
                    StorepinkteddybearBdContext.Instance.Orders.Remove(order);
                    StorepinkteddybearBdContext.Instance.SaveChanges();
                }
            }

            return Ok("Товар удален из корзины");
        }

        [HttpPut("UpdateQuantity")]
        public ActionResult UpdateQuantity(int orderItemId, int newQuantity)
        {
            var orderItem = StorepinkteddybearBdContext.Instance.Orderitems.Find(orderItemId);

            if (orderItem == null)
                return NotFound("Товар в корзине не найден");

            var toy = StorepinkteddybearBdContext.Instance.Toys
                .FirstOrDefault(t => t.ArticulToy == orderItem.ArticulToy);

            if (toy == null)
                return NotFound("Товар не найден");

            if (toy.QuantityInStock < newQuantity)
                return BadRequest($"Недостаточно товара на складе. Доступно: {toy.QuantityInStock}");

            if (newQuantity <= 0)
                return BadRequest("Количество должно быть больше 0");

            orderItem.Quantity = newQuantity;
            StorepinkteddybearBdContext.Instance.Orderitems.Update(orderItem);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            UpdateOrderTotalAmount(orderItem.IdOrder);

            return Ok("Количество обновлено");
        }


        [HttpPost("{orderId}/Checkout")]
        public ActionResult<Order> Checkout(int orderId, string address)
        {
            if (!string.IsNullOrEmpty(address)) return BadRequest("Добавьте к заказу адрес");
            var order = StorepinkteddybearBdContext.Instance.Orders
                .Include(o => o.Orderitems)
                .ThenInclude(oi => oi.ArticulToyNavigation)
                .FirstOrDefault(o => o.IdOrder == orderId && o.StatusOrder == "ожидает подтверждения");

            if (order == null)
                return NotFound("Корзина не найдена");

            order.AdressOrder = address;
            order.StatusOrder = "в обработке";

            foreach (var item in order.Orderitems)
            {
                item.ArticulToyNavigation.QuantityInStock -= item.Quantity;
                StorepinkteddybearBdContext.Instance.Toys.Update(item.ArticulToyNavigation);
            }

            StorepinkteddybearBdContext.Instance.Orders.Update(order);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(order);
        }

        private void UpdateOrderTotalAmount(int orderId)
        {
            var order = StorepinkteddybearBdContext.Instance.Orders.Find(orderId);
            if (order == null) return;
            order.TotalAmount = order.Orderitems.Sum(oi => oi.Quantity * oi.UnitPrice);
            StorepinkteddybearBdContext.Instance.Orders.Update(order);
            StorepinkteddybearBdContext.Instance.SaveChanges();
        }
    }
}