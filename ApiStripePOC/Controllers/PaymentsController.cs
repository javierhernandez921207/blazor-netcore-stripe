using ApiStripePOC.Model;
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
                PaymentMethodTypes = new List<string> { "card", "link" },
                Usage = "off_session",
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

                // Create the Payment Intent
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
        [HttpPost("create-subscription")]
        public ActionResult<SubscriptionCreateResponse> CreateSubscription([FromBody] CreateSubscriptionRequest req)
        {
            var customerId = req.CustomerId;

            // Automatically save the payment method to the subscription
            // when the first payment is successful.
            var paymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = "on_subscription",
            };

            // Create the subscription. Note we're expanding the Subscription's
            // latest invoice and that invoice's payment_intent
            // so we can pass it to the front end to confirm the payment
            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = req.PriceId                        
                    },
                },
                Discounts = new List<SubscriptionDiscountOptions>() { new SubscriptionDiscountOptions() { PromotionCode = "promo_1QUxjEEKndZkgAQGKFrxaA7V" } },
                TrialPeriodDays = 7,
                PaymentSettings = paymentSettings,
                PaymentBehavior = "default_incomplete",
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");
            var subscriptionService = new SubscriptionService();
            try
            {
                Subscription subscription = subscriptionService.Create(subscriptionOptions);

                return new SubscriptionCreateResponse
                {
                    SubscriptionId = subscription.Id
                };
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Failed to create subscription.{e}");
                return BadRequest();
            }
        }
    }
}
