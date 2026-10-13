using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellphones_backend.DTOs.Requests;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cellphones_backend.Services.Implement
{
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    public class ReviewServiceImpl : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<User> _userManager;

        public ReviewServiceImpl(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            UserManager<User> userManager)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        public async Task<ReviewResponse> CreateReviewAsync(CreateReviewRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            var review = new Review
            {
                UserId = userId,
                ProductId = request.ProductId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedDate = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

            var response = new ReviewResponse(
                review.Id,
                review.UserId,
                user.Email ?? "",
                user.UserName ?? "",
                review.ProductId,
                review.Rating,
                review.Comment,
                review.CreatedDate
            );

            return response;
        }

        public async Task<List<ReviewResponse>> GetReviewsByProductIdAsync(long productId)
        {
            var reviews = await _reviewRepository.GetReviewsWithUserByProductIdAsync(productId);
            var responseList = reviews.Select(r => new ReviewResponse(
                r.Id,
                r.UserId,
                r.User?.Email ?? "",
                r.User?.UserName ?? "",
                r.ProductId,
                r.Rating,
                r.Comment,
                r.CreatedDate
            )).ToList();

            return responseList;
        }

        public async Task<double> GetAverageRatingAsync(long productId)
        {
            var reviews = await _reviewRepository.FindAsync(r => r.ProductId == productId);
            double avg = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            return Math.Round(avg, 1);
        }
    }
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
