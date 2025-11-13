using Microsoft.AspNetCore.Mvc;
using StoreTeddyBear.Models;
using Microsoft.EntityFrameworkCore;

namespace StoreTeddyBear.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        [HttpPost("Add")]
        public ActionResult<Review> AddReview([FromBody] ReviewRequest request)
        {
            var errors = GetValidationErrors(request);
            if (errors.Count > 0)
                return BadRequest($"Некорректные данные:\n\n{string.Join("\n", errors)}");

            var customer = StorepinkteddybearBdContext.Instance.Useransadmins
                .FirstOrDefault(c => c.IdCustomer == request.CustomerId && c.StatusUsersProfile == "активный");

            if (customer == null)
                return NotFound("Пользователь не найден");

            var toy = StorepinkteddybearBdContext.Instance.Toys
                .FirstOrDefault(t => t.ArticulToy == request.ArticulToy);

            if (toy == null)
                return NotFound("Товар не найден");

            var existingReview = StorepinkteddybearBdContext.Instance.Reviews
                .FirstOrDefault(r => r.ArticulToy == request.ArticulToy && r.IdCustomer == request.CustomerId);

            if (existingReview != null)
                return BadRequest("Вы уже оставляли отзыв на этот товар");

            var review = new Review
            {
                ArticulToy = request.ArticulToy,
                IdCustomer = request.CustomerId,
                RatingReview = request.Rating,
                CommentReview = request.Comment,
                DateReview = DateTime.Now
            };

            StorepinkteddybearBdContext.Instance.Reviews.Add(review);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(review);
        }

        [HttpGet("GetByProduct/{articulToy}")]
        public ActionResult<List<ReviewResponse>> GetReviewsByProduct(string articulToy)
        {
            var reviews = StorepinkteddybearBdContext.Instance.Reviews
                .Include(r => r.IdCustomerNavigation)
                .Where(r => r.ArticulToy == articulToy)
                .OrderByDescending(r => r.DateReview)
                .ToList();

            var response = reviews.Select(r => new ReviewResponse
            {
                IdReview = r.IdReview,
                CustomerName = r.IdCustomerNavigation.NameUsers,
                Rating = r.RatingReview,
                Comment = r.CommentReview,
                Date = r.DateReview
            }).ToList();

            return Ok(response);
        }

        [HttpGet("GetByCustomer/{customerId}")]
        public ActionResult<List<Review>> GetReviewsByCustomer(int customerId)
        {
            var reviews = StorepinkteddybearBdContext.Instance.Reviews
                .Include(r => r.ArticulToyNavigation)
                .Where(r => r.IdCustomer == customerId)
                .OrderByDescending(r => r.DateReview)
                .ToList();

            return Ok(reviews);
        }

        [HttpPut("Edit/{reviewId}")]
        public ActionResult<Review> EditReview(int reviewId, [FromBody] EditReviewRequest request)
        {
            var review = StorepinkteddybearBdContext.Instance.Reviews
                .FirstOrDefault(r => r.IdReview == reviewId);

            if (review == null)
                return NotFound("Отзыв не найден");

            if (review.DateReview.HasValue && (DateTime.Now - review.DateReview.Value).TotalDays > 1)
            {
                return BadRequest("Редактирование отзыва возможно только в течение 24 часов после его создания");
            }

            if (!review.DateReview.HasValue)
            {
                return BadRequest("Невозможно редактировать отзыв: дата создания не указана");
            }

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Рейтинг должен быть от 1 до 5");

            review.RatingReview = request.Rating;
            review.CommentReview = request.Comment;
            review.DateReview = DateTime.Now;

            StorepinkteddybearBdContext.Instance.Reviews.Update(review);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok(review);
        }

        [HttpDelete("Delete/{reviewId}")]
        public ActionResult DeleteReview(int reviewId)
        {
            var review = StorepinkteddybearBdContext.Instance.Reviews
                .FirstOrDefault(r => r.IdReview == reviewId);

            if (review == null)
                return NotFound("Отзыв не найден");

            StorepinkteddybearBdContext.Instance.Reviews.Remove(review);
            StorepinkteddybearBdContext.Instance.SaveChanges();

            return Ok("Отзыв удален");
        }

        [HttpGet("AverageRating/{articulToy}")]
        public ActionResult<AverageRatingResponse> GetAverageRating(string articulToy)
        {
            var reviews = StorepinkteddybearBdContext.Instance.Reviews
                .Where(r => r.ArticulToy == articulToy);

            if (!reviews.Any())
                return Ok(new AverageRatingResponse { AverageRating = 0, ReviewCount = 0 });

            var averageRating = reviews.Average(r => (double)r.RatingReview);
            var reviewCount = reviews.Count();

            var response = new AverageRatingResponse
            {
                AverageRating = Math.Round((decimal)averageRating, 1),
                ReviewCount = reviewCount
            };

            return Ok(response);
        }

        private List<string> GetValidationErrors(ReviewRequest request)
        {
            var errors = new List<string>();

            if (request.Rating < 1 || request.Rating > 5)
                errors.Add("Рейтинг должен быть от 1 до 5");

            if (string.IsNullOrWhiteSpace(request.Comment))
                errors.Add("Комментарий не может быть пустым");

            if (request.Comment.Length > 1000)
                errors.Add("Комментарий не должен превышать 1000 символов");

            return errors;
        }
    }

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