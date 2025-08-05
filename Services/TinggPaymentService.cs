using LocalGov360.Data;
using LocalGov360.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class TinggPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _http;
    private readonly ILogger<TinggPaymentService> _logger;

    public TinggPaymentService(
        ApplicationDbContext context,
        HttpClient http,
        ILogger<TinggPaymentService> logger)
    {
        _context = context;
        _http = http;
        _logger = logger;
    }

    public async Task<ServicePayment> InitiateTinggPaymentAsync(
        Guid organisationId,
        Guid workflowInstanceId,
        Guid stepId,
        int serviceId,
        decimal amount,
        string createdBy)
    {
        try
        {
            // Load Tingg config
            var config = await _context.TinggConfigurations
                .FirstOrDefaultAsync(c => c.OrganisationId == organisationId);

            if (config == null)
            {
                var msg = $"Tingg configuration not found for organisation {organisationId}";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            // Get Auth Token
            var token = await GetAuthToken(config);
            _logger.LogInformation("Obtained auth token.");

            // Generate transaction reference and account
            var transactionRef = Guid.NewGuid().ToString("N");
            var accountNumber = $"SVC-{serviceId}-{DateTime.UtcNow.Ticks}";

            // Static/fixed test user info (replace in production)
            var customerFirstName = "Brian";
            var customerLastName = "Chanda";
            var customerPhone = "260968365822";
            var customerEmail = "bc@gmil.com";

            // Express Checkout payload
            var payload = new
            {
                customer_first_name = customerFirstName,
                customer_last_name = customerLastName,
                msisdn = customerPhone,
                account_number = accountNumber,
                request_amount = amount.ToString("0.00"),
                merchant_transaction_id = transactionRef,
                service_code = config.ServiceCode,
                country_code = config.CountryCode,
                currency_code = config.CurrencyCode,
                callback_url = config.CallbackUrl?.Trim(),
                success_redirect_url = config.SuccessRedirectUrl?.Trim(),
                fail_redirect_url = config.FailRedirectUrl?.Trim(),
                customer_email = customerEmail,
                request_description = "Municipal service payment"
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var requestBody = JsonSerializer.Serialize(payload, jsonOptions);
            _logger.LogInformation("Prepared express checkout payload.");

            // Save pending payment record
            var payment = new ServicePayment
            {
                WorkflowInstanceId = workflowInstanceId,
                WorkflowStepId = stepId,
                OrganisationId = organisationId,
                ServiceId = serviceId,
                Amount = amount,
                Currency = config.CurrencyCode,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                Status = "Pending",
                PaymentMethod = "Tingg",
                TransactionReference = transactionRef,
                RequestPayload = requestBody
            };

            _context.ServicePayments.Add(payment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved pending payment record.");

            // Send Express Checkout request
            var request = new HttpRequestMessage(HttpMethod.Post, config.CheckoutRequestUrl);
            request.Headers.Add("ApiKey", config.ApiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Received express checkout response: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Response content: {ResponseContent}", responseContent);

            payment.ResponsePayload = responseContent;
            payment.Status = response.IsSuccessStatusCode ? "Pending" : "Failed";

            if (response.IsSuccessStatusCode)
            {
                var checkoutUrl = ExtractCheckoutUrl(responseContent);
                if (!string.IsNullOrEmpty(checkoutUrl))
                {
                    payment.PaymentUrl = checkoutUrl;
                    _logger.LogInformation("Extracted checkout URL: {CheckoutUrl}", checkoutUrl);
                }
                else
                {
                    _logger.LogWarning("Checkout URL not found in response.");
                }
            }
            else
            {
                _logger.LogError("Express checkout request failed.");
            }

            await _context.SaveChangesAsync();
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during express checkout initiation.");
            throw;
        }
    }

    private async Task<string> GetAuthToken(TinggConfiguration config)
    {
        var payload = new
        {
            client_id = config.ClientId,
            client_secret = config.ClientSecret,
            grant_type = "client_credentials"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, config.AuthTokenRequestUrl);
        request.Headers.Add("ApiKey", config.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var err = $"Auth failed: {response.StatusCode} - {content}";
            _logger.LogError(err);
            throw new Exception(err);
        }

        using var jsonDoc = JsonDocument.Parse(content);
        if (jsonDoc.RootElement.TryGetProperty("access_token", out var tokenProp) ||
            jsonDoc.RootElement.TryGetProperty("accessToken", out tokenProp))
        {
            return tokenProp.GetString() ?? throw new Exception("Empty access token");
        }

        throw new Exception("Access token not found in auth response.");
    }

    private string? ExtractCheckoutUrl(string json)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(json);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("data", out var data) &&
                data.TryGetProperty("checkoutUrl", out var longUrl) &&
                !string.IsNullOrEmpty(longUrl.GetString()))
            {
                return longUrl.GetString();
            }

            if (root.TryGetProperty("results", out var results) &&
                results.TryGetProperty("short_url", out var shortUrl) &&
                !string.IsNullOrEmpty(shortUrl.GetString()))
            {
                return shortUrl.GetString();
            }

            if (root.TryGetProperty("checkoutUrl", out var url) && !string.IsNullOrEmpty(url.GetString()))
            {
                return url.GetString();
            }

            if (root.TryGetProperty("checkout_url", out var url2) && !string.IsNullOrEmpty(url2.GetString()))
            {
                return url2.GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse checkout URL.");
            return null;
        }
    }
}
