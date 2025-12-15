using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        public PaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        // GET: api/payments/list/{page}/{pageSize}
        [HttpGet("list/{page}/{pageSize}")]
        public Task<ActionResult<ApiResponse<List<Payment>>>> GetPayments(int page, int pageSize)
        {
            return null!; // TODO: implement listing with pagination
        }

        // GET: api/payments/{id}
        [HttpGet("{id}")]
        public Task<ActionResult<ApiResponse<Payment>>> GetPayment(long id)
        {
            return null!; // TODO: implement single payment retrieval
        }

        // POST: api/payments
        [HttpPost]
        public Task<ActionResult<ApiResponse<string>>> CreatePayment([FromBody] Payment payment)
        {
            return null!; // TODO: implement payment creation
        }

        // PUT: api/payments/{id}
        [HttpPut("{id}")]
        public Task<ActionResult<ApiResponse<string>>> UpdatePayment(long id, [FromBody] Payment payment)
        {
            return null!; // TODO: implement payment update
        }

        // DELETE: api/payments/{id}
        [HttpDelete("{id}")]
        public Task<ActionResult<ApiResponse<string>>> DeletePayment(long id)
        {
            return null!; // TODO: implement payment deletion
        }
    }
}
