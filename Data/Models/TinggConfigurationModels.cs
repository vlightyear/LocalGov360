using System;
using System.ComponentModel.DataAnnotations;

namespace LocalGov360.Data.Models
{
    public class TinggConfiguration
    {
        public Guid Id { get; set; }
        public Guid? OrganisationId { get; set; }
        [Required(ErrorMessage = "API Base URL is required")]
        public string ApiBaseUrl { get; set; } = string.Empty;
        [Required(ErrorMessage = "API Key is required")]
        public string ApiKey { get; set; } = string.Empty;
        [Required(ErrorMessage = "Auth Token Request URL is required")]
        public string AuthTokenRequestUrl { get; set; } = string.Empty;
        [Required(ErrorMessage = "Callback URL is required")]
        public string CallbackUrl { get; set; } = string.Empty;
        [Required(ErrorMessage = "Checkout Request URL is required")]
        public string CheckoutRequestUrl { get; set; } = string.Empty;
        [Required(ErrorMessage = "Client ID is required")]
        public string ClientId { get; set; } = string.Empty;
        [Required(ErrorMessage = "Client Secret is required")]
        public string ClientSecret { get; set; } = string.Empty;
        [Required(ErrorMessage = "Country Code is required")]
        public string CountryCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Currency Code is required")]
        public string CurrencyCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Fail Redirect URL is required")]
        public string FailRedirectUrl { get; set; } = string.Empty;
        [Required(ErrorMessage = "Payment Mode Code is required")]
        public string PaymentModeCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Service Code is required")]
        public string ServiceCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Success Redirect URL is required")]
        public string SuccessRedirectUrl { get; set; } = string.Empty;
        public Organisation? Organisation { get; set; }
    }
}