using Microsoft.AspNetCore.Mvc;
using StoreTeddyBear.Models;
using Microsoft.EntityFrameworkCore;

namespace StoreTeddyBear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpGet("CustomerOrders/{customerId}")]
        public ActionResult<List<OrderResponse>> GetCustomerOrders(int customerId)
        {
            var orders = StorepinkteddybearBdContext.Instance.Orders
                .Include(o => o.Orderitems)
                .ThenInclude(oi => oi.ArticulToyNavigation)
                .Where(o => o.IdCustomer == customerId)
                .OrderByDescending(o => o.DateOrder)
                .ToList();

            var response = orders.Select(o => new OrderResponse
            {
                IdOrder = o.IdOrder,
                DateOrder = o.DateOrder,
                StatusOrder = o.StatusOrder,
                AdressOrder = o.AdressOrder,
                TotalAmount = o.TotalAmount ?? 0,
                Items = o.Orderitems.Select(oi => new OrderItemResponse
                {
                    Title = oi.ArticulToyNavigation.Title,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            }).ToList();

            return Ok(response);
        }

        [HttpGet("Details/{orderId}")]
        public ActionResult<OrderResponse> GetOrderDetails(int orderId)
        {
            var order = StorepinkteddybearBdContext.Instance.Orders
                .Include(o => o.Orderitems)
                .ThenInclude(oi => oi.ArticulToyNavigation)
                .FirstOrDefault(o => o.IdOrder == orderId);

            if (order == null)
                return NotFound("Заказ не найден");

            var response = new OrderResponse
            {
                IdOrder = order.IdOrder,
                DateOrder = order.DateOrder,
                StatusOrder = order.StatusOrder,
                AdressOrder = order.AdressOrder,
                TotalAmount = order.TotalAmount ?? 0,
                Items = order.Orderitems.Select(oi => new OrderItemResponse
                {
                    Title = oi.ArticulToyNavigation.Title,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return Ok(response);
        }

       

        [HttpPut("UpdateStatus/{orderId}")]
        public ActionResult<Order> UpdateOrderStatus(int orderId, [FromBody] UpdateStatusRequest request)
        {
            var order = StorepinkteddybearBdContext.Instance.Orders
                .FirstOrDefault(o => o.IdOrder == orderId);

            if (order == null)
                return NotFound("Заказ не найден");

            var validStatuses = new[] { "ожидает подтверждения", "в обработке", "отгружен", "доставлен" };
            if (!validStatuses.Contains(request.NewStatus.ToLower()))
                return BadRequest("Некорректный статус заказа");

            order.StatusOrder = request.NewStatus.ToLower();
            StorepinkteddybearBdContext.Instance.Orders.Update(order);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(order);
        }

        
    }

    public class OrderResponse
    {
        public int IdOrder { get; set; }
        public DateTime? DateOrder { get; set; }
        public string StatusOrder { get; set; }
        public string AdressOrder { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponse> Items { get; set; }
    }

    public class OrderItemResponse
    {
        public string Title { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; }
    }
}