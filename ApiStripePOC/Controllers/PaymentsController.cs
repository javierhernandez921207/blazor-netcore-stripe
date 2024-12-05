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
        public IActionResult CreateSetupIntent()
        {
            var service = new SetupIntentService();            
            var setupIntent = service.Create(new SetupIntentCreateOptions
            {
                //PaymentMethodTypes = new List<string> { "card", "amazon_pay", "acss_debit" },
                Usage = "off_session"
            });

            return Ok(new { ClientSecret = setupIntent.ClientSecret });
        }

        [HttpPost("register-customer")]
        public IActionResult RegisterCustomer([FromBody] CustomerRequest request)
        {
            try
            {
                // Crear el cliente en Stripe
                var customerService = new CustomerService();
                var customer = customerService.Create(new CustomerCreateOptions
                {
                    Name = request.Name,
                    Email = request.Email,
                });

                // Asociar el método de pago al cliente
                var paymentMethodService = new PaymentMethodService();
                paymentMethodService.Attach(request.PaymentMethodId, new PaymentMethodAttachOptions
                {
                    Customer = customer.Id,
                });

                // Opcional: Establecer el método como predeterminado
                customerService.Update(customer.Id, new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = request.PaymentMethodId,
                    }
                });

                return Ok(new { Message = "Client and payment methods registered successfully", CustomerId = customer.Id });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("charge-customer")]
        public async Task<IActionResult> ChargeCustomer([FromBody] ChargeRequest request)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();

                // Crear el Payment Intent
                var options = new PaymentIntentCreateOptions
                {
                    Customer = request.CustomerId,
                    Amount = request.AmountInCents,
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethodId,
                    OffSession = true, // Si no necesitas interacción del cliente
                    Confirm = true,    // Confirmar la transacción automáticamente
                };

                var paymentIntent = await paymentIntentService.CreateAsync(options);

                return Ok(new
                {
                    Success = true,
                    PaymentIntentId = paymentIntent.Id,
                    Status = paymentIntent.Status
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }
        public class CustomerRequest
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string PaymentMethodId { get; set; }
        }
        public class ChargeRequest
        {
            public string CustomerId { get; set; }
            public string PaymentMethodId { get; set; }
            public long AmountInCents { get; set; }
            public string Currency { get; set; } = "usd";
            public string Description { get; set; } = "Cobro desde API";
        }
    }
}
