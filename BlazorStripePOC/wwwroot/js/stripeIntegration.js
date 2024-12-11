let stripe;
let elements;
let paymentElement;
async function initializeStripe(clientSecret) {
    stripe = Stripe('pk_test_51QPQWNEKndZkgAQG3XyOCoqJSFNzy6StDBWUpNZGmax0YSun2QQqEj0zIo981ZsyvOhLpT5JXgzgJS7MO7mk2Fvm004uo6PFfO', {
        apiVersion: "2024-11-20.acacia",
        stripeAccount: 'acct_1QPQWNEKndZkgAQG',
    });
    const appearance = {
        theme: '',
        variables: { colorPrimaryText: '#262626' }
    };
    const options = {
        fields: {
            billingDetails: {
                address: 'auto',
            }
        },
        layout: {
            type: 'tabs'
        }
    };
    elements = stripe.elements({ clientSecret, appearance });
    paymentElement = elements.create('payment', options);
    paymentElement.mount("#payment-element");

    return { success: true };
}
async function confirmPM() {
    // Confirmar el método de pago con Stripe
    const { setupIntent, error } = await stripe.confirmSetup({
        elements,
        redirect: "if_required"
    });

    if (error) {
        console.error(error.message);
        return null
    }
    else
        return setupIntent.payment_method
}
