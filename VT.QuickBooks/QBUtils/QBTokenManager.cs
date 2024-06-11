using Newtonsoft.Json;
using System.Net;
using VT.Common;
using VT.QuickBooks.DTOs;

namespace VT.QuickBooks.QBUtils
{
    public static class QBTokenManager
    {
        public static RefreshTokenResponse GetAccessToken(string authorizationToken, bool isUnauthorized = false)
        {
            var response = new RefreshTokenResponse();
            if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken) || isUnauthorized)
            {
                // make token generating call
                response = RefreshToken(authorizationToken);
                if (response.Success)
                    ApplicationSettings.GetAccessToken = response.AccessToken;
            }
            return response;
        }

        private static RefreshTokenResponse RefreshToken(string authorizationToken)
        {
            var response = new RefreshTokenResponse();
            try
            {
                var client = new QBRestClient
                {
                    ContentType = ApplicationSettings.QBTokenContentType,
                    EndPoint = ApplicationSettings.QBTokenAccessURI,
                    Method = HttpVerb.POST,
                    PostData = "grant_type=refresh_token&refresh_token=" + ApplicationSettings.QBRefreshToken + "",
                    Token = authorizationToken
                };
                response = client.MakeTokenRefreshRequest();
                if (response.Success)
                {
                    response = JsonConvert.DeserializeObject<RefreshTokenResponse>(response.ResponseValue);
                    response.Success = !string.IsNullOrEmpty(response.AccessToken);
                }
            }
            catch (WebException ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }
    }
}
