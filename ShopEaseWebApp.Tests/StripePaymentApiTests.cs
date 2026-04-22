using Microsoft.Extensions.Configuration;
using Stripe;

namespace ShopEaseWebApp.Tests;

public sealed class StripePaymentApiTests
{
    private const string SuccessCardNumber = "4242424242424242";
    private const string DeclinedCardNumber = "4000000000000002";

    [Fact]
    public async Task StripePayment_Succeeds_WithValidTestCard()
    {
        var result = await MakeTestPaymentAsync(SuccessCardNumber);

        Assert.Equal("successful payment", result);
    }

    [Fact]
    public async Task StripePayment_Fails_WithDeclinedTestCard()
    {
        var result = await MakeTestPaymentAsync(DeclinedCardNumber);

        Assert.Equal("card failed", result);
    }

    private static async Task<string> MakeTestPaymentAsync(string cardNumber)
    {
        var key = new ConfigurationBuilder()
            .AddUserSecrets<StripePaymentApiTests>()
            .Build()["Stripe:SecretKey"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Stripe test key not configured in test-project User Secrets. Set Stripe:SecretKey.");
        }

        StripeConfiguration.ApiKey = key;

        // Stripe blocks raw PAN API submission by default, so we map known test card numbers to Stripe test payment methods.
        var paymentMethodId = cardNumber switch
        {
            SuccessCardNumber => "pm_card_visa",
            DeclinedCardNumber => "pm_card_chargeDeclined",
            _ => throw new ArgumentException("Unsupported Stripe test card number.", nameof(cardNumber))
        };

        var paymentIntentService = new PaymentIntentService();

        try
        {
            var intent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = 500,
                Currency = "gbp",
                PaymentMethod = paymentMethodId,
                Confirm = true,
                PaymentMethodTypes = new List<string> { "card" }
            });

            return string.Equals(intent.Status, "succeeded", StringComparison.OrdinalIgnoreCase)
                ? "successful payment"
                : "card failed";
        }
        catch (StripeException)
        {
            return "card failed";
        }
    }
}
