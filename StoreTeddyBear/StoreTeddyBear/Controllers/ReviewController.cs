using Microsoft.AspNetCore.Mvc;
using StoreTeddyBear.Models;
using Microsoft.EntityFrameworkCore;

namespace StoreTeddyBear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //Добавление отзыва
    //Получение отзывов определенного товара
    //Получение отзывов, который оставил конкретный пользователь
    //Редактирование отзыва
    //Удаление отзыва
    //Получение среднего рейтинга товара

    public class ReviewController : ControllerBase
    {
        [HttpPost("Add")]
        public ActionResult<Review> AddReview(int customerId, string ArticulToy, int rating, string comment)
        {
            var errors = GetValidationErrors(rating, comment);
            if (errors.Count > 0)
                return BadRequest($"Некорректные данные:\n\n{string.Join("\n", errors)}");

            var customer = StorepinkteddybearBdContext.Instance.Useransadmins
                .FirstOrDefault(c => c.IdCustomer == customerId && c.StatusUsersProfile == "активный");

            if (customer == null)
                return NotFound("Пользователь не найден");

            var toy = StorepinkteddybearBdContext.Instance.Toys
                .FirstOrDefault(t => t.ArticulToy == ArticulToy);

            if (toy == null)
                return NotFound("Товар не найден");

            var existingReview = StorepinkteddybearBdContext.Instance.Reviews
                .FirstOrDefault(r => r.ArticulToy == ArticulToy && r.IdCustomer == customerId);

            if (existingReview != null)
                return BadRequest("Вы уже оставляли отзыв на этот товар");

            var review = new Review
            {
                ArticulToy = ArticulToy,
                IdCustomer = customerId,
                RatingReview = (sbyte)rating,
                CommentReview = comment,
                DateReview = DateTime.Now
            };

            StorepinkteddybearBdContext.Instance.Reviews.Add(review);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(review);
        }

        [HttpGet("{articulToy}/GetByProduct")]
        public ActionResult<List<Review>> GetReviewsByProduct(string articulToy)
        {
            var reviews = StorepinkteddybearBdContext.Instance.Reviews
                .Include(r => r.IdCustomerNavigation)
                .Where(r => r.ArticulToy == articulToy)
                .OrderByDescending(r => r.DateReview)
                .ToList();

            return Ok(reviews);
        }

        [HttpGet("{customerId}/GetByCustomer")]
        public ActionResult<List<Review>> GetReviewsByCustomer(int customerId)
        {
            var reviews = StorepinkteddybearBdContext.Instance.Reviews
                .Include(r => r.ArticulToyNavigation)
                .Where(r => r.IdCustomer == customerId)
                .OrderByDescending(r => r.DateReview)
                .ToList();

            return Ok(reviews);
        }

        [HttpPut("{reviewId}/Edit")]
        public ActionResult<Review> EditReview(int reviewId, int rating, string comment)
        {
            var review = StorepinkteddybearBdContext.Instance.Reviews
                .Find(reviewId);


            if (review == null)
                return NotFound("Отзыв не найден");

           if (!review.DateReview.HasValue)
                return BadRequest("Невозможно редактировать отзыв: дата создания не указана");

            if ((DateTime.Now - review.DateReview.Value).TotalDays > 1)
                return BadRequest("Редактирование отзыва возможно только в течение 24 часов после его создания");

            if (rating < 1 || rating > 5)
                return BadRequest("Рейтинг должен быть от 1 до 5");

            review.RatingReview = (sbyte)rating;
            review.CommentReview = comment;
            review.DateReview = DateTime.Now;

            StorepinkteddybearBdContext.Instance.Reviews.Update(review);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(review);
        }

        [HttpDelete("{reviewId}/Delete")]
        public ActionResult DeleteReview(int reviewId)
        {
            var review = StorepinkteddybearBdContext.Instance.Reviews
                .Find(reviewId);

            if (review == null)
                return NotFound("Отзыв не найден");

            StorepinkteddybearBdContext.Instance.Reviews.Remove(review);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok("Отзыв удален");
        }

        [HttpGet("{articulToy}/AverageRating")]
        public ActionResult<AverageRatingBear> GetAverageRating(string articulToy)
        {
            var reviews = StorepinkteddybearBdContext.Instance.Reviews
                .Where(r => r.ArticulToy == articulToy);

            decimal averageRating = 0;
            int reviewCount = 0;

            if (!reviews.Any())
                return Ok(new AverageRatingBear {AverageRating = averageRating,ReviewCount = reviewCount});

            averageRating = reviews.Average(r => (decimal)r.RatingReview);
            reviewCount = reviews.Count();

            var response = new AverageRatingBear
            {
                AverageRating = Math.Round((decimal)averageRating, 1),
                ReviewCount = reviewCount
            };

            return Ok(response);
        }

        private List<string> GetValidationErrors(int rating, string comment)
        {
            var errors = new List<string>();

            if (rating < 1 || rating > 5)
                errors.Add("Рейтинг должен быть от 1 до 5");

            if (string.IsNullOrWhiteSpace(comment))
                errors.Add("Комментарий не может быть пустым");

            if (comment.Length > 1000)
                errors.Add("Комментарий не должен превышать 1000 символов");

            return errors;
        }
    }

    public class AverageRatingBear
    {
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}