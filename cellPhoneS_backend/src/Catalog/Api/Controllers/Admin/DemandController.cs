using System.Security.Claims;
using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/demands")]
    [ApiController]
    [RequirePermission(Permissions.CatalogRead)]
    public class AdminDemandController : BaseController
    {
        private readonly DemandService _demandService;

        private readonly IAdminCatalogQueryService _queryService;

        public AdminDemandController(DemandService demandService, IAdminCatalogQueryService queryService)
        {
            _demandService = demandService;
            _queryService = queryService;
        }

        // GET: api/admin/demands?pageIndex=1&pageSize=20&search=&status=&categoryId=&sortBy=createDate&sortDir=desc
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminDemandView>>>> GetDemands([FromQuery] AdminCatalogFilter filter)
        {
            var result = await _queryService.GetDemands(filter);
            return Ok(new ApiResponse<PagedResult<AdminDemandView>>("Success", result));
        }

        // GET: api/admin/demands/{demandId}
        [HttpGet("{demandId:long}")]
        public async Task<ActionResult<ApiResponse<AdminDemandView>>> GetDemand(long demandId)
        {
            var result = await _queryService.GetDemand(demandId);
            return Ok(new ApiResponse<AdminDemandView>("Success", result));
        }

        // PATCH: api/admin/demands/{demandId}/restore   deleted -> active
        [HttpPatch("{demandId:long}/restore")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> RestoreDemand(long demandId)
        {
            await _demandService.RestoreDemand(demandId, CurrentUserId);
            return Ok(new ApiResponse<string>("Demand restored successfully", ""));
        }

        // POST: api/admin/demands
        [HttpPost]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<long>>> CreateDemand([FromBody] CreateDemandRequest createDemandRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _demandService.CreateDemand(createDemandRequest, userId);
            return Ok(new ApiResponse<long>("Demand created successfully", result));
        }

        // PUT: api/admin/demands/{demandId}
        [HttpPut("{demandId}")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateDemand(
            long demandId, 
            [FromBody] UpdateDemandRequest updateDemandRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            // Prevent ID tampering
            if (demandId != updateDemandRequest.Id)
            {
                return BadRequest(new ApiResponse<string>("Demand ID in URL does not match ID in request body", null!));
            }
            
            await _demandService.UpdateDemand(demandId, updateDemandRequest, userId);
            return Ok(new ApiResponse<string>("Demand updated successfully", ""));
        }

        // DELETE: api/admin/demands/{demandId}
        [HttpDelete("{demandId}")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteDemand(long demandId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            await _demandService.DeleteDemand(demandId, userId);
            return Ok(new ApiResponse<string>("Demand deleted successfully", ""));
        }
    }
}
