using Newtonsoft.Json;

namespace VT.QuickBooks.DTOs
{
    public class RefreshTokenResponse : QuickbookBaseResponse
    {
        [JsonProperty(PropertyName = "expires_in")]
        public int AccessTokenExpireIn { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "x_refresh_token_expires_in")]
        public int RefreshTokenExpireIn { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
    }
}
