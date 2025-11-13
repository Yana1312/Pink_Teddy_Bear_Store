using Castle.Core.Resource;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreTeddyBear.Models;
using System.Text.RegularExpressions;
using System.Xml.Linq;

//For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StoreTeddyBear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        
        [HttpGet("GetAllCustomers")]
        public ActionResult<List<Useransadmin>> GetAllCustomers()
        {
            return Ok(StorepinkteddybearBdContext.Instance.Useransadmins.ToList());
        }

        [HttpGet("{id}")]
        public ActionResult<Useransadmin> GetChooseCustomer(int id)
        {
            Useransadmin? customer = StorepinkteddybearBdContext.Instance.Useransadmins.Find(id);
            return customer == null ? NotFound("Покупатель не найден") : Ok(customer);
        }

        [HttpPost("Authorization")]
        public ActionResult<Useransadmin> Authorization(string email, string password)
        {
            var customer = StorepinkteddybearBdContext.Instance.Useransadmins.FirstOrDefault(cus => cus.EmailUsers == email);
            var errors = GetErrorsAuth(password, email, customer);
            if (errors.Count > 0)
                return BadRequest($"Некорректные данные:\n\n{string.Join("\n", errors)}");
            return Ok(customer);
        }

        public static List<string> GetValidationErrors(Useransadmin customer)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(customer.NameUsers))
                errors.Add("Имя не может быть пустым");
            if (string.IsNullOrWhiteSpace(customer.EmailUsers))
                errors.Add("Почта не может быть пустой");
            if (!Regex.IsMatch(customer.EmailUsers, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(com|gmail|ru)$", RegexOptions.IgnoreCase))
                errors.Add("Введите корректную почту");
            if (customer.NameUsers.Length < 2)
                errors.Add("Минимальная длина имени - 2 буквы");
            if (!string.IsNullOrWhiteSpace(customer.PasswordHash) && customer.PasswordHash.Length < 8)
                errors.Add("Пароль не должен быть меньше 8 символов");
            return errors;
        }


        public static List<string> GetErrorsAuth(string password, string email, Useransadmin customer)
        {
            var errors = new List<string>();
            if (customer == null) return new List<string> { "Пользователь не найден"};

            if (email != customer.EmailUsers || !BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
                errors.Add("Некорректные данные, попробуйте ещё раз");

            if (customer.StatusUsersProfile == "неактивный")
                errors.Add("Данный аккаунт перестал существовать");

            errors.AddRange(GetValidationErrors(Useransadmin.CreateUser(email, customer.NameUsers, password)));

            return errors;
        }

        [HttpPost("Registration")]
        public ActionResult<Useransadmin> Registration([FromBody] Useransadmin customer)
        {
            var errors = GetValidationErrors(customer);
            var existingCustomer = StorepinkteddybearBdContext.Instance.Useransadmins.FirstOrDefault(c =>
            c.EmailUsers.Equals(customer.EmailUsers, StringComparison.OrdinalIgnoreCase));

            if (existingCustomer != null)
            {
                if (existingCustomer.StatusUsersProfile?.ToLower() == "активный")
                    errors.Add("Данная почта уже зарегистрирована");
                else if (existingCustomer.StatusUsersProfile?.ToLower() == "неактивный")
                    errors.Add("Данная почта зарегистрирована, но не активна");
            }

            if (errors.Count > 0)
                return BadRequest($"Некорректные данные:\n\n{string.Join("\n", errors)}");

            customer.StatusUsersProfile = "активный";
            if (customer.RoleUsers?.Trim().ToLower() != "админ")
                customer.RoleUsers = "пользователь";
            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(customer.PasswordHash);
            StorepinkteddybearBdContext.Instance.Useransadmins.Add(customer);
            StorepinkteddybearBdContext.Instance.SaveChanges();
            return Ok(customer);
        }

        [HttpPatch("{id}/EditProfile")]
        public ActionResult<Useransadmin> EditProfile(int id, [FromBody] Useransadmin updatedCustomer)
        {
            if (id != updatedCustomer.IdCustomer) return BadRequest("Ваши уникальные ключи не совпадают");

            var currentCustomer = StorepinkteddybearBdContext.Instance.Useransadmins.Find(updatedCustomer.IdCustomer);
            if (currentCustomer == null) return NotFound("Пользователь не найден");

            var errors = GetValidationErrors(updatedCustomer);
            var existCustomer = StorepinkteddybearBdContext.Instance.Useransadmins.FirstOrDefault(c => c.EmailUsers.Equals(updatedCustomer.EmailUsers, StringComparison.OrdinalIgnoreCase) &&
                                                            c.IdCustomer != updatedCustomer.IdCustomer);

            if (existCustomer != null)
                    errors.Add("Данная почта уже используется другим пользователем");

            if (errors.Count > 0)
                return BadRequest($"Некорректные данные:\n\n{string.Join("\n", errors)}");

            currentCustomer.NameUsers = updatedCustomer.NameUsers;
            currentCustomer.EmailUsers = updatedCustomer.EmailUsers;

            if (!BCrypt.Net.BCrypt.Verify(updatedCustomer.PasswordHash, currentCustomer.PasswordHash))
                currentCustomer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedCustomer.PasswordHash);

            StorepinkteddybearBdContext.Instance.SaveChanges();
            return Ok(currentCustomer);
        }

        [HttpPatch("{id}/Deactivate")]
        public ActionResult DeactivateCustomer(int id)
        {
            var currentCustomer = StorepinkteddybearBdContext.Instance.Useransadmins.Find(id);
            if (currentCustomer == null)
                return NotFound("Пользователь не найден");
            if (currentCustomer.StatusUsersProfile == "неактивный") return BadRequest("Пользователь уже деактивирован");
            currentCustomer.StatusUsersProfile = "неактивный";
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok("Пользователь успешно деактивирован");
        }
    }
}
