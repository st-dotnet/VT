using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using VT.Common;
using VT.Common.Utils;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.Customers;
using VT.QuickBooks.Interfaces;
using VT.QuickBooks.QBUtils;

namespace VT.QuickBooks.Services
{
    public class CustomerServices : ICustomer
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

        public QuickbookBaseResponse UpdateCustomer(string jsonCustomer, string authorizationToken)
        {
            var response = new QuickbookBaseResponse();
            var tokenRefreshResponse = new RefreshTokenResponse();
            bool isMakeCall = false;

            try
            {
                if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken))
                {
                    tokenRefreshResponse = QBTokenManager.GetAccessToken(authorizationToken);

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
                        EndPoint = string.Format("{0}/v3/company/{1}/customer/", BaseUrl, RealmId),
                        Method = QBUtils.HttpVerb.POST,
                        PostData = jsonCustomer,
                        Token = ApplicationSettings.GetAccessToken
                    };
                    response = client.MakeQBRequest();
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        // create customer on quick books
        public QuickbookBaseResponse CreateCustomer(string jsonCustomer, string authorizationTokenHeader)
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
                        EndPoint = string.Format("{0}/v3/company/{1}/customer/", BaseUrl, RealmId),
                        Method = QBUtils.HttpVerb.POST,
                        PostData = jsonCustomer,
                        Token = ApplicationSettings.GetAccessToken
                    };
                    response = client.MakeQBRequest();
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public QuickbookBaseResponse DeleteCustomer(int customerId)
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

        public GetCustomerResponse GetCustomer(string customerId, string authorizationToken)
        {
            var response = new GetCustomerResponse();
            var tokenRefreshResponse = new RefreshTokenResponse();
            bool isMakeCall = false;
            try
            {
                if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken))
                {
                    tokenRefreshResponse = QBTokenManager.GetAccessToken(authorizationToken);

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
                        EndPoint = string.Format("{0}/v3/company/{1}/customer/{2}", BaseUrl, RealmId, customerId),
                        Method = QBUtils.HttpVerb.GET,
                        PostData = "",
                        Token = ApplicationSettings.GetAccessToken
                    };
                    var qbResponse = client.MakeQBRequest();
                    if (qbResponse.Success)
                        response = XmlUtil.Deserialize<GetCustomerResponse>(qbResponse.ResponseValue);
                }

                // DO POST SUCCESS STUFF//
            }
            catch (Exception ex)
            {
            }
            return response;
        }

        public AllCustomerResponse GetAllCustomers(string authorizationToken)
        {
            var response = new AllCustomerResponse();
            var qbResponse = new QuickbookBaseResponse();
            var tokenRefreshResponse = new RefreshTokenResponse();
            bool isMakeCall = false;

            try
            {
                // token generating access
                if (string.IsNullOrEmpty(ApplicationSettings.GetAccessToken))
                {
                    tokenRefreshResponse = QBTokenManager.GetAccessToken(authorizationToken);

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
                    var query = "select * from customer";
                    var encodeQuery = WebUtility.UrlEncode(query);
                    string uri = string.Format("{0}/v3/company/{1}/query?query={2}", BaseUrl, RealmId, encodeQuery);
                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = uri,
                        Method = QBUtils.HttpVerb.GET,
                        PostData = "",
                        Token = ApplicationSettings.GetAccessToken
                    };
                    qbResponse = client.MakeQBRequest();
                }
                response = XmlUtil.Deserialize<AllCustomerResponse>(qbResponse.ResponseValue);
                response.Success = true;
                response.Message = "Customers Fetched successfully.";
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