document.addEventListener('DOMContentLoaded', async () => {
    const stripe = Stripe('pk_test_51QPQWNEKndZkgAQG3XyOCoqJSFNzy6StDBWUpNZGmax0YSun2QQqEj0zIo981ZsyvOhLpT5JXgzgJS7MO7mk2Fvm004uo6PFfO', {
        apiVersion: "2024-11-20.acacia",
        stripeAccount: 'acct_1QPQWNEKndZkgAQG',
    });

    const paymentRequest = stripe.paymentRequest({
        country: 'US',
        currency: 'usd',
        total: {
            label: 'Demo total',
            amount: 1000,
        },
        requestPayerName: true,
        requestPayerEmail: true,
    });

    const elements = stripe.elements();
    const prButton = elements.create('paymentRequestButton', {
        paymentRequest: paymentRequest,
    });

    paymentRequest.canMakePayment().then(result => {
        console.log(paymentRequest);
        if (result) {
            prButton.mount('#payment-request-button');
        }
        else {
            document.getElementById('payment-request-button').style.display = 'none';
            console.log('Payment Request API is not available.');
        }
    });

    paymentRequest.on('paymentmethod', async (ev) => {
        console.log(ev.paymentMethod);
    });
});