using LocalGov360.Data;
using LocalGov360.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

public class TinggPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _http;

    public TinggPaymentService(ApplicationDbContext context, HttpClient http)
    {
        _context = context;
        _http = http;
    }

    public async Task<ServicePaymentsModels> InitiateTinggPaymentAsync(
        Guid organisationId,
        Guid workflowInstanceId,
        Guid stepId,
        Guid serviceId,
        decimal amount,
        string createdBy)
    {
        // 1. Load Tingg configuration
        var config = await _context.TinggConfigurations
            .FirstOrDefaultAsync(c => c.OrganisationId == organisationId);

        if (config == null)
            throw new Exception("Tingg configuration not found for this organisation.");

        // 2. Get auth token
        var token = await GetAuthToken(config);

        // 3. Build request
        var transactionRef = Guid.NewGuid().ToString();

        var tinggRequest = new
        {
            merchantTransactionID = transactionRef,
            amount = amount,
            currencyCode = config.CurrencyCode,
            countryCode = config.CountryCode,
            serviceCode = config.ServiceCode,
            clientId = config.ClientId,
            callbackUrl = config.CallbackUrl,
            returnUrl = config.SuccessRedirectUrl,
            cancelUrl = config.FailRedirectUrl,
            paymentMode = config.PaymentModeCode,
            accountNumber = $"SVC-{serviceId}-{DateTime.UtcNow.Ticks}"
        };

        var payload = JsonSerializer.Serialize(tinggRequest);

        // 4. Send request to Tingg
        var request = new HttpRequestMessage(HttpMethod.Post, config.CheckoutRequestUrl);
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to initiate Tingg payment: " + responseContent);

        // 5. Save payment record
        var payment = new ServicePaymentsModels
        {
            WorkflowInstanceId = workflowInstanceId,
            WorkflowStepId = stepId,
            ServiceId = serviceId,
            Amount = amount,
            Currency = config.CurrencyCode,
            CreatedBy = createdBy,
            Status = "Pending",
            PaymentMethod = "Tingg",
            TransactionReference = transactionRef,
            PaymentUrl = GetRedirectUrlFromResponse(responseContent),
            RequestPayload = payload,
            ResponsePayload = responseContent
        };

        _context.ServicePayments.Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    private async Task<string> GetAuthToken(TinggConfiguration config)
    {
        var credentials = new
        {
            clientId = config.ClientId,
            clientSecret = config.ClientSecret
        };

        var payload = JsonSerializer.Serialize(credentials);

        var response = await _http.PostAsync(
            config.AuthTokenRequestUrl,
            new StringContent(payload, Encoding.UTF8, "application/json"));

        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to get Tingg auth token: " + responseString);

        var json = JsonSerializer.Deserialize<JsonElement>(responseString);
        return json.GetProperty("accessToken").GetString() ?? throw new Exception("Token missing");
    }

    private string? GetRedirectUrlFromResponse(string json)
    {
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);
        if (parsed.TryGetProperty("checkoutUrl", out var url))
        {
            return url.GetString();
        }
        return null;
    }
}
