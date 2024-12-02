let stripe;
let elements;
let paymentElement;

async function initializeStripe(publishableKey, clientSecret) {
    stripe = Stripe(publishableKey);
    elements = stripe.elements({
        clientSecret: clientSecret 
    });
    paymentElement = elements.create("payment");
    paymentElement.mount("#payment-element");

    return { success: true };
}

async function confirmPaymentMethod(clientSecret) {
    if (!stripe || !paymentElement) {
        return { success: false, error: "Stripe not initialized." };
    }

    const { error, setupIntent } = await stripe.confirmSetup({
        clientSecret: clientSecret,
        elements,
    });

    if (error) {
        return { success: false, error: error.message };
    }

    return { success: true, paymentMethodId: setupIntent.payment_method };
}
