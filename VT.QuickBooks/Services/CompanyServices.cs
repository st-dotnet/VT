using System;
using System.Net;
using VT.Common;
using VT.Common.Utils;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.CompanyServices;
using VT.QuickBooks.Interfaces;
using VT.QuickBooks.QBUtils;

namespace VT.QuickBooks.Services
{
    public class CompanyServices : IQBCompanyService
    {
        #region Properties

        public static string CompanyAcessToken { get; set; }

        public string BaseUrl
        {
            get { return ApplicationSettings.QBBaseUrl; }
        }

        public string RealmId
        {
            get { return ApplicationSettings.QBRealmId; }
        }

        #endregion

        #region Public Methods

        public QuickbookBaseResponse CreateQBCompanyService(string jsonService, string authorizationTokenHeader)
        {
            var response = new QuickbookBaseResponse();
            var tokenRefreshResponse = new RefreshTokenResponse();
            bool isMakeCall = false;
            try
            {
                if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken))
                {
                    tokenRefreshResponse = QBTokenManager.GetAccessToken(authorizationTokenHeader);

                    if (!tokenRefreshResponse.Success)
                    {
                        response.Success = false;
                        response.Message = $"Token error : {response.Message}";
                        return response;
                    }
                    isMakeCall = true;
                }
                else
                {
                    // access token already existing
                    isMakeCall = true;
                }
                if (isMakeCall)
                {
                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = string.Format("{0}/v3/company/{1}/item/", BaseUrl, RealmId),
                        Method = QBUtils.HttpVerb.POST,
                        PostData = jsonService,
                        Token = ApplicationSettings.GetAccessToken
                    };
                    response = client.MakeQBRequest();
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
                response.Success = false;
            }
            return response;
        }

        public QuickbookBaseResponse DeleteQBCompanyService(string serviceId)
        {
            var response = new QuickbookBaseResponse();
            try
            {

            }
            catch (Exception ex)
            {

                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public GetQBServiceResponse GetQBService(string serviceId, string authorizationTokenHeader)
        {
            var response = new GetQBServiceResponse();
            var tokenRefreshResponse = new RefreshTokenResponse();
            bool isMakeCall = false;
            try
            {
                if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken))
                {
                    tokenRefreshResponse = QBTokenManager.GetAccessToken(authorizationTokenHeader);

                    if (!tokenRefreshResponse.Success)
                    {
                        response.Success = false;
                        response.Message = $"Token error : {tokenRefreshResponse.Message}";
                        return response;
                    }
                    isMakeCall = true;
                }
                else
                {
                    // access token already existing
                    isMakeCall = true;
                }
                if (isMakeCall)
                {
                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = string.Format("{0}/v3/company/{1}/item/{2}", BaseUrl, RealmId, serviceId),
                        Method = QBUtils.HttpVerb.GET,
                        PostData = "",
                        Token = ApplicationSettings.GetAccessToken
                    };
                    var qbResponse = client.MakeQBRequest();
                    if (qbResponse.Success)
                    {
                        response = XmlUtil.Deserialize<GetQBServiceResponse>(qbResponse.ResponseValue);
                        response.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public GetAllServices GetAllServices(string authorizationTokenHeader)
        {
            var servicesResponse = new GetAllServices();
            var response = new QuickbookBaseResponse();
            var tokenRefreshResponse = new RefreshTokenResponse();
            bool isMakeCall = false;

            try
            {
                // token generating access
                if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken))
                {
                    tokenRefreshResponse = QBTokenManager.GetAccessToken(authorizationTokenHeader);

                    if (!tokenRefreshResponse.Success)
                    {
                        servicesResponse.Success = false;
                        servicesResponse.Message = $"Token error : {tokenRefreshResponse.Message}";
                        return servicesResponse;
                    }
                    isMakeCall = true;
                }
                else
                {
                    // access token already exists
                    isMakeCall = true;
                }
                if (isMakeCall)
                {
                    var query = "select * from item";
                    var encodedQuery = WebUtility.HtmlEncode(query);
                    string uri = string.Format("{0}/v3/company/{1}/query?query={2}", BaseUrl, RealmId, encodedQuery);

                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = uri,
                        Method = QBUtils.HttpVerb.GET,
                        Token = ApplicationSettings.GetAccessToken,
                        PostData = ""
                    };
                    response = client.MakeQBRequest();
                }
                if (!response.Success)
                {
                    servicesResponse.Success = false;
                    servicesResponse.Message = response.Message;
                    return servicesResponse;
                }
                servicesResponse = XmlUtil.Deserialize<GetAllServices>(response.ResponseValue);
                servicesResponse.Success = response.Success;
                servicesResponse.Message = response.Message;
            }
            catch (Exception ex)
            {
                servicesResponse.Success = false;
                servicesResponse.Message = ex.Message.ToString();
            }
            return servicesResponse;
        }

        public QuickbookBaseResponse UpdateService(string jsonService, string authorizationTokenHeader)
        {
            var response = new QuickbookBaseResponse();
            var tokenRefreshResponse = new RefreshTokenResponse();
            bool isMakeCall = false;

            try
            {
                // token generating access
                if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken))
                {
                    tokenRefreshResponse = QBTokenManager.GetAccessToken(authorizationTokenHeader);

                    if (!tokenRefreshResponse.Success)
                    {
                        response.Success = false;
                        response.Message = $"Token error : {tokenRefreshResponse.Message}";
                        return response;
                    }
                    isMakeCall = true;
                }
                else
                {
                    // access token already existing
                    isMakeCall = true;
                }
                if (isMakeCall)
                {
                    string uri = string.Format("{0}/v3/company/{1}/item", BaseUrl, RealmId);

                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = uri,
                        Method = QBUtils.HttpVerb.POST,
                        PostData = jsonService,
                        Token = ApplicationSettings.GetAccessToken
                    };
                    response = client.MakeQBRequest();
                    response.Success = true;                   
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        #endregion
    }
}
