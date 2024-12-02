using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
namespace ApiStripePOC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        [HttpPost("create-setup-intent")]
        public IActionResult CreateSetupIntent([FromBody] string customerId)
        {
            try
            {
                var setupIntentService = new SetupIntentService();
                var setupIntent = setupIntentService.Create(new SetupIntentCreateOptions
                {
                    Customer = customerId,
                    PaymentMethodTypes = new List<string> { "card" }
                });

                return Ok(new { ClientSecret = setupIntent.ClientSecret });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("create-customer")]
        public IActionResult CreateCustomer([FromBody] CustomerRequest request)
        {
            try
            {
                var customerService = new CustomerService();
                var customer = customerService.Create(new CustomerCreateOptions
                {
                    Name = request.Name,
                    Email = request.Email,
                    PaymentMethod = request.PaymentMethodId,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = request.PaymentMethodId
                    }
                });

                return Ok(new { CustomerId = customer.Id });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("register-payment-method")]
        public IActionResult RegisterPaymentMethod([FromBody] RegisterPaymentRequest request)
        {
            try
            {
                var customerService = new CustomerService();
                customerService.Update(request.CustomerId, new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = request.PaymentMethodId
                    }
                });

                return Ok(new { Message = "Método de pago registrado con éxito." });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }  
        [HttpPost("process-payment")]
        public IActionResult ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Create(new PaymentIntentCreateOptions
                {
                    Amount = request.Amount,
                    Currency = "usd",
                    Customer = request.CustomerId,
                    PaymentMethod = request.PaymentMethodId,
                    Confirm = true,
                    OffSession = true
                });

                return Ok(new { PaymentIntentId = paymentIntent.Id });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
    public class RegisterPaymentRequest
    {
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
    }
    public class CustomerRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PaymentMethodId { get; set; }
    }

    public class PaymentRequest
    {
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
        public long Amount { get; set; }
    }
}
