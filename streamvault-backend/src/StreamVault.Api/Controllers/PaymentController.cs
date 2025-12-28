using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Payments;
using StreamVault.Application.Payments.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("payment-intent")]
    public async Task<ActionResult<PaymentIntentDto>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var paymentIntent = await _paymentService.CreatePaymentIntentAsync(request, userId, tenantId);
            return Ok(paymentIntent);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("payment-intent/{paymentIntentId}")]
    public async Task<ActionResult<PaymentIntentDto>> GetPaymentIntent(string paymentIntentId)
    {
        try
        {
            var paymentIntent = await _paymentService.GetPaymentIntentAsync(paymentIntentId);
            return Ok(paymentIntent);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirm-payment/{paymentIntentId}")]
    public async Task<IActionResult> ConfirmPayment(string paymentIntentId)
    {
        try
        {
            var success = await _paymentService.ConfirmPaymentAsync(paymentIntentId);
            return Ok(new { success });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("process-subscription")]
    public async Task<ActionResult<SubscriptionPaymentResultDto>> ProcessSubscriptionPayment([FromBody] ProcessSubscriptionPaymentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _paymentService.ProcessSubscriptionPaymentAsync(request, userId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult<List<PaymentMethodDto>>> GetPaymentMethods()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var paymentMethods = await _paymentService.GetPaymentMethodsAsync(userId, tenantId);
            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("payment-methods")]
    public async Task<ActionResult<PaymentMethodDto>> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var paymentMethod = await _paymentService.AddPaymentMethodAsync(request, userId, tenantId);
            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("payment-methods/{paymentMethodId}")]
    public async Task<IActionResult> RemovePaymentMethod(string paymentMethodId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var success = await _paymentService.RemovePaymentMethodAsync(paymentMethodId, userId, tenantId);
            return Ok(new { success });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactionHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var transactions = await _paymentService.GetTransactionHistoryAsync(userId, tenantId, page, pageSize);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
