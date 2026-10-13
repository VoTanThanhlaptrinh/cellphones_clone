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

        public async Task<ServiceResult<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<ReviewResponse>.Fail("User not found", ServiceErrorType.NotFound);
            }

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return ServiceResult<ReviewResponse>.Fail("Product not found", ServiceErrorType.NotFound);
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

            return ServiceResult<ReviewResponse>.Success(response, "Review submitted successfully");
        }

        public async Task<ServiceResult<List<ReviewResponse>>> GetReviewsByProductIdAsync(long productId)
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

            return ServiceResult<List<ReviewResponse>>.Success(responseList, "Reviews retrieved successfully");
        }

        public async Task<ServiceResult<double>> GetAverageRatingAsync(long productId)
        {
            var reviews = await _reviewRepository.FindAsync(r => r.ProductId == productId);
            double avg = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            return ServiceResult<double>.Success(Math.Round(avg, 1), "Average rating retrieved successfully");
        }
    }
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
