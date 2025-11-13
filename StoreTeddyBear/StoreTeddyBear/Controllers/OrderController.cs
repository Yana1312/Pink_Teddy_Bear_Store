using Microsoft.AspNetCore.Mvc;
using StoreTeddyBear.Models;
using Microsoft.EntityFrameworkCore;

namespace StoreTeddyBear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //Получение всех заказов покупателя
    //Получение деталей заказа
    //Обновление статуса передвижения заказа
    public class OrderController : ControllerBase
    {
        [HttpGet("{customerId}/CustomerOrders")]
        public ActionResult<List<Order>> GetCustomerOrders(int customerId)
        {
            var orders = StorepinkteddybearBdContext.Instance.Orders
                .Include(o => o.Orderitems)
                .ThenInclude(oi => oi.ArticulToyNavigation)
                .Where(o => o.IdCustomer == customerId)
                .OrderByDescending(o => o.DateOrder)
                .ToList();

            if (orders == null) return NotFound("Заказы не найдены");

            return Ok(orders);
        }

        [HttpGet("{orderId}/Details")]
        public ActionResult<Order> GetOrderDetails(int orderId)
        {
            var order = StorepinkteddybearBdContext.Instance.Orders
                .Include(o => o.Orderitems)
                .ThenInclude(oi => oi.ArticulToyNavigation)
                .FirstOrDefault(o => o.IdOrder == orderId);

            if (order == null)
                return NotFound("Заказ не найден");

            return Ok(order);
        }

        [HttpPut("{orderId}/UpdateStatus")]
        public ActionResult<Order> UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = StorepinkteddybearBdContext.Instance.Orders
                .Find(orderId);

            if (order == null)
                return NotFound("Заказ не найден");

            var validStatuses = new[] { "ожидает подтверждения", "в обработке", "отгружен", "доставлен" };
            if (!validStatuses.Contains(newStatus.ToLower()))
                return BadRequest("Некорректный статус заказа");

            order.StatusOrder = newStatus.ToLower();
            StorepinkteddybearBdContext.Instance.Orders.Update(order);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(order);
        }
    }
}