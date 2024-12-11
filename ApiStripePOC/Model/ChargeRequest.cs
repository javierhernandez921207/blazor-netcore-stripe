namespace ApiStripePOC.Model
{
    public class ChargeRequest
    {
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
        public long AmountInCents { get; set; }
        public string Currency { get; set; } = "usd";
        public string Description { get; set; } = "Cobro desde API";
    }
}
