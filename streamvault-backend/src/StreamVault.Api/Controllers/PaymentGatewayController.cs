using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StreamVault.Application.Payments;
using StreamVault.Application.Payments.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentGatewayController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<PaymentGatewayController> _logger;

    public PaymentGatewayController(IPaymentGatewayService paymentGatewayService, ILogger<PaymentGatewayController> logger)
    {
        _paymentGatewayService = paymentGatewayService;
        _logger = logger;
    }

    [HttpPost("payment-intents")]
    public async Task<ActionResult<PaymentIntentDto>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            var paymentIntent = await _paymentGatewayService.CreatePaymentIntentAsync(request);
            return Ok(paymentIntent);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("payment-intents/{paymentIntentId}/confirm")]
    public async Task<ActionResult<PaymentIntentDto>> ConfirmPayment(string paymentIntentId)
    {
        try
        {
            var paymentIntent = await _paymentGatewayService.ConfirmPaymentAsync(paymentIntentId);
            return Ok(paymentIntent);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("payment-intents/{paymentIntentId}/cancel")]
    public async Task<ActionResult<bool>> CancelPayment(string paymentIntentId)
    {
        try
        {
            var result = await _paymentGatewayService.CancelPaymentAsync(paymentIntentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("refunds")]
    public async Task<ActionResult<RefundDto>> CreateRefund([FromBody] CreateRefundRequest request)
    {
        try
        {
            var refund = await _paymentGatewayService.CreateRefundAsync(request);
            return Ok(refund);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("payment-methods")]
    public async Task<ActionResult<PaymentMethodDto>> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request)
    {
        try
        {
            var paymentMethod = await _paymentGatewayService.CreatePaymentMethodAsync(request);
            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("payment-methods/{paymentMethodId}")]
    public async Task<ActionResult<bool>> DeletePaymentMethod(string paymentMethodId)
    {
        try
        {
            var result = await _paymentGatewayService.DeletePaymentMethodAsync(paymentMethodId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("customers/{customerId}/payment-methods")]
    public async Task<ActionResult<List<PaymentMethodDto>>> GetCustomerPaymentMethods(string customerId, [FromQuery] int? limit = null)
    {
        try
        {
            var paymentMethods = await _paymentGatewayService.GetCustomerPaymentMethodsAsync(customerId, limit);
            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("subscriptions")]
    public async Task<ActionResult<SubscriptionDto>> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            var subscription = await _paymentGatewayService.CreateSubscriptionAsync(request);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("subscriptions/{subscriptionId}")]
    public async Task<ActionResult<SubscriptionDto>> UpdateSubscription(string subscriptionId, [FromBody] UpdateSubscriptionRequest request)
    {
        try
        {
            var subscription = await _paymentGatewayService.UpdateSubscriptionAsync(subscriptionId, request);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("subscriptions/{subscriptionId}/cancel")]
    public async Task<ActionResult<bool>> CancelSubscription(string subscriptionId, [FromQuery] bool immediate = false)
    {
        try
        {
            var result = await _paymentGatewayService.CancelSubscriptionAsync(subscriptionId, immediate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("invoices/{invoiceId}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(string invoiceId)
    {
        try
        {
            var invoice = await _paymentGatewayService.GetInvoiceAsync(invoiceId);
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("customers/{customerId}/invoices")]
    public async Task<ActionResult<List<InvoiceDto>>> GetCustomerInvoices(string customerId, [FromQuery] int? limit = null)
    {
        try
        {
            var invoices = await _paymentGatewayService.GetCustomerInvoicesAsync(customerId, limit);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("customers")]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var customer = await _paymentGatewayService.CreateCustomerAsync(request);
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("customers/{customerId}")]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(string customerId, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            var customer = await _paymentGatewayService.UpdateCustomerAsync(customerId, request);
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("customers/{customerId}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(string customerId)
    {
        try
        {
            var customer = await _paymentGatewayService.GetCustomerAsync(customerId);
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("webhooks/stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSecretKey = Request.Headers["Stripe-Signature"];
            
            // In production, verify webhook signature
            // var webhookSecret = _configuration["Stripe:WebhookSecret"];
            // var stripeEvent = EventUtility.ConstructEvent(json, stripeSecretKey, webhookSecret);

            // For now, just parse the event
            var stripeEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<WebhookEventDto>(json);
            
            // Handle different event types
            switch (stripeEvent?.Type)
            {
                case "payment_intent.succeeded":
                    // Handle successful payment
                    break;
                case "payment_intent.payment_failed":
                    // Handle failed payment
                    break;
                case "invoice.payment_succeeded":
                    // Handle successful invoice payment
                    break;
                case "invoice.payment_failed":
                    // Handle failed invoice payment
                    break;
                case "customer.subscription.created":
                    // Handle subscription creation
                    break;
                case "customer.subscription.updated":
                    // Handle subscription update
                    break;
                case "customer.subscription.deleted":
                    // Handle subscription deletion
                    break;
                default:
                    // Unhandled event type
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return BadRequest();
        }
    }
}
