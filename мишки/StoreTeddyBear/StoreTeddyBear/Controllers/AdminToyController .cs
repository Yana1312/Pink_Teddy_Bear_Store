using Microsoft.AspNetCore.Mvc;
using StoreTeddyBear.Models;

namespace StoreTeddyBear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminToyController : ControllerBase
    {
        [HttpPost("Add")]
        public ActionResult<Toy> AddToy([FromBody] Toy toy)
        {
            var errors = GetToyValidationErrors(toy);
            if (errors.Count > 0)
                return BadRequest($"Некорректные данные:\n\n{string.Join("\n", errors)}");

            var existingToy = StorepinkteddybearBdContext.Instance.Toys
                .FirstOrDefault(t => t.ArticulToy == toy.ArticulToy);

            if (existingToy != null)
                return BadRequest("Товар с таким артикулом уже существует");

            StorepinkteddybearBdContext.Instance.Toys.Add(toy);

            var inventory = new Inventory
            {
                ArticulToy = toy.ArticulToy,
                QuantityToys = toy.QuantityInStock
            };
            StorepinkteddybearBdContext.Instance.Inventories.Add(inventory);

            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(toy);
        }

        [HttpPut("Update/{articulToy}")]
        public ActionResult<Toy> UpdateToy(string articulToy, [FromBody] Toy updatedToy)
        {
            var existingToy = StorepinkteddybearBdContext.Instance.Toys
                .FirstOrDefault(t => t.ArticulToy == articulToy);

            if (existingToy == null)
                return NotFound("Товар не найден");

            var errors = GetToyValidationErrors(updatedToy);
            if (errors.Count > 0)
                return BadRequest($"Некорректные данные:\n\n{string.Join("\n", errors)}");

            existingToy.Title = updatedToy.Title;
            existingToy.Descriptionn = updatedToy.Descriptionn;
            existingToy.Price = updatedToy.Price;
            existingToy.Height = updatedToy.Height;
            existingToy.Weight = updatedToy.Weight;
            existingToy.QuantityInStock = updatedToy.QuantityInStock;

            var inventory = StorepinkteddybearBdContext.Instance.Inventories
                .FirstOrDefault(i => i.ArticulToy == articulToy);

            if (inventory != null)
            {
                inventory.QuantityToys = updatedToy.QuantityInStock;
                StorepinkteddybearBdContext.Instance.Inventories.Update(inventory);
            }

            StorepinkteddybearBdContext.Instance.Toys.Update(existingToy);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(existingToy);
        }

        [HttpDelete("Delete/{articulToy}")]
        public ActionResult DeleteToy(string articulToy)
        {
            var toy = StorepinkteddybearBdContext.Instance.Toys
                .FirstOrDefault(t => t.ArticulToy == articulToy);

            if (toy == null)
                return NotFound("Товар не найден");

            var activeOrderItems = StorepinkteddybearBdContext.Instance.Orderitems
                .Any(oi => oi.ArticulToy == articulToy &&
                          oi.IdOrderNavigation.StatusOrder != "доставлен");

            if (activeOrderItems)
                return BadRequest("Нельзя удалить товар, так как он есть в активных заказах");

            StorepinkteddybearBdContext.Instance.Toys.Remove(toy);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok("Товар удален");
        }

       

        [HttpGet("Search")]
        public ActionResult<List<Toy>> SearchToys(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Поисковый запрос не может быть пустым");

            var toys = StorepinkteddybearBdContext.Instance.Toys
                .Where(t => t.Title.Contains(searchTerm) ||
                           t.Descriptionn.Contains(searchTerm) ||
                           t.ArticulToy.Contains(searchTerm))
                .ToList();

            return Ok(toys);
        }

        private List<string> GetToyValidationErrors(Toy toy)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(toy.ArticulToy))
                errors.Add("Артикул не может быть пустым");

            if (string.IsNullOrWhiteSpace(toy.Title))
                errors.Add("Название не может быть пустым");

            if (toy.Price < 0)
                errors.Add("Цена не может быть отрицательной");

            if (toy.QuantityInStock < 0)
                errors.Add("Количество на складе не может быть отрицательным");

            if (toy.Price > 1000000)
                errors.Add("Цена слишком высокая");

            return errors;
        }
    }
}