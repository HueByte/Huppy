using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NetherNet.Api.Auth
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private enum AuthenticationFailureReason
        {
            NONE = 0,
            API_KEY_HEADER_NOT_PROVIDED,
            API_KEY_HEADER_VALUE_NULL,
            API_KEY_INVALID
        }

        private AuthenticationFailureReason _failureReason = AuthenticationFailureReason.NONE;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IOptionsMonitor<ApiKeyAuthenticationOptions> _settings;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            ILogger<ApiKeyAuthenticationHandler> logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config) : base(options, loggerFactory, encoder, clock)
        {
            _logger = logger;
            _config = config;
            _settings = options;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //ApiKey header get
            if (!TryGetApiKeyHeader(out string providedApiKey, out AuthenticateResult authenticateResult))
                return authenticateResult;

            //TODO: you apikey validity check
            if (await ApiKeyCheckAsync(providedApiKey))
            {
                var principal = new ClaimsPrincipal();  //TODO: Create your Identity retreiving claims
                var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.Scheme);

                return AuthenticateResult.Success(ticket);
            }

            _failureReason = AuthenticationFailureReason.API_KEY_INVALID;
            return AuthenticateResult.NoResult();
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            //Create response
            Response.Headers.Append(HeaderNames.WWWAuthenticate, $@"Authorization realm=""{ApiKeyAuthenticationOptions.DefaultScheme}""");
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.ContentType = MediaTypeNames.Application.Json;

            var result = new
            {
                StatusCode = Response.StatusCode,
                Message = _failureReason switch
                {
                    AuthenticationFailureReason.API_KEY_HEADER_NOT_PROVIDED => "ApiKey not provided",
                    AuthenticationFailureReason.API_KEY_HEADER_VALUE_NULL => "ApiKey value is null",
                    AuthenticationFailureReason.NONE or AuthenticationFailureReason.API_KEY_INVALID or _ => "ApiKey is not valid"
                }
            };

            using var responseStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(responseStream, result);
            await Response.BodyWriter.WriteAsync(responseStream.ToArray());
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            //Create response
            Response.Headers.Append(HeaderNames.WWWAuthenticate, $@"Authorization realm=""{ApiKeyAuthenticationOptions.DefaultScheme}""");
            Response.StatusCode = StatusCodes.Status403Forbidden;
            Response.ContentType = MediaTypeNames.Application.Json;

            var result = new
            {
                Response.StatusCode,
                Message = "Forbidden"
            };

            using var responseStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(responseStream, result);
            await Response.BodyWriter.WriteAsync(responseStream.ToArray());
        }

        #region Privates
        private bool TryGetApiKeyHeader(out string apiKeyHeaderValue, out AuthenticateResult result)
        {
            apiKeyHeaderValue = null!;
            if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeaderValues))
            {
                _logger.LogError("ApiKey header not provided");

                _failureReason = AuthenticationFailureReason.API_KEY_HEADER_NOT_PROVIDED;
                result = AuthenticateResult.Fail("ApiKey header not provided");

                return false;
            }

            apiKeyHeaderValue = apiKeyHeaderValues.FirstOrDefault()!;
            if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(apiKeyHeaderValue))
            {
                _logger.LogError("ApiKey header value null");

                _failureReason = AuthenticationFailureReason.API_KEY_HEADER_VALUE_NULL;
                result = AuthenticateResult.Fail("ApiKey header value null");

                return false;
            }

            result = null!;
            return true;
        }

        private Task<bool> ApiKeyCheckAsync(string apiKey)
        {
            //TODO: setup your validation code...
            if (!(apiKey == _settings.CurrentValue.AuthKey))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
        #endregion
    }

    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public static string Scheme => DefaultScheme;
        public static string AuthenticationType => DefaultScheme;
        public string? AuthKey { get; set; }
    }

    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> options)
            => authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options);
    }
}