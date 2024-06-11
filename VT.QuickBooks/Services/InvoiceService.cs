using System;
using VT.Common;
using VT.Common.Utils;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.Invoices;
using VT.QuickBooks.Interfaces;
using VT.QuickBooks.QBUtils;

namespace VT.QuickBooks.Services
{
    public class InvoiceService : IInvoices
    {
        #region Properties

        public string BaseUrl
        {
            get { return ApplicationSettings.QBBaseUrl; }
        }

        public string AccessToken
        {
            get { return ApplicationSettings.AccessToken; }
        }

        public string RealmId
        {
            get { return ApplicationSettings.QBRealmId; }
        }

        #endregion

        #region Public Methods

        public QuickbookBaseResponse CreateInvoice(string jsonInvoiceCreation, string authorizationToken)
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
                        EndPoint = string.Format("{0}/v3/company/{1}/invoice", BaseUrl, RealmId),
                        Token = ApplicationSettings.GetAccessToken,
                        Method = QBUtils.HttpVerb.POST,
                        PostData = jsonInvoiceCreation
                    };
                    // make quickbook request
                    var quickbookResponse = client.MakeQBRequest();

                    response.Success = quickbookResponse.Success;
                    response.Message = quickbookResponse.Message;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public DeleteInvoiceResponse DeleteInvoice(string jsonDeleteInvoice)
        {
            var response = new DeleteInvoiceResponse();
            try
            {
                var client = new QBRestClient
                {
                    ContentType = "application/json",
                    EndPoint = string.Format("{0}/v3/company/{1}/invoice?operation=delete", BaseUrl, RealmId),
                    Token = AccessToken,
                    Method = QBUtils.HttpVerb.POST,
                    PostData = jsonDeleteInvoice
                };
                var qbDeleteInvoiceResponse = client.MakeQBRequest();
                if (qbDeleteInvoiceResponse.Success)
                    response = XmlUtil.Deserialize<DeleteInvoiceResponse>(qbDeleteInvoiceResponse.ResponseValue);
            }
            catch (Exception ex)
            {
            }
            return response;
        }

        public GetInvoiceResponse GetInvoice(int invoiceId)
        {
            var response = new GetInvoiceResponse();
            try
            {
                var client = new QBRestClient
                {
                    ContentType = "application/json",
                    EndPoint = string.Format("{0}/v3/company/{1}/invoice/{2}", BaseUrl, RealmId, invoiceId),
                    Token = AccessToken,
                    Method = QBUtils.HttpVerb.GET,
                    PostData = ""
                };
                var qbBaseResponse = client.MakeQBRequest();
                if (qbBaseResponse.Success)
                    response = XmlUtil.Deserialize<GetInvoiceResponse>(qbBaseResponse.ResponseValue);
            }
            catch (Exception ex)
            {
            }
            return response;
        }

        public VoidInvoiceResponse VoidInvoice(string jsonVoidRequest)
        {
            var response = new VoidInvoiceResponse();
            try
            {
                var client = new QBRestClient
                {
                    ContentType = "application/json",
                    EndPoint = string.Format("{0}/v3/company/{1}/invoice?operation=void", BaseUrl, RealmId),
                    Token = AccessToken,
                    Method = QBUtils.HttpVerb.POST,
                    PostData = jsonVoidRequest
                };
                var voidResponse = client.MakeQBRequest();
                if (voidResponse.Success)
                    response = XmlUtil.Deserialize<VoidInvoiceResponse>(voidResponse.ResponseValue);
            }
            catch (Exception ex)
            {
            }
            return response;
        }

        #endregion
    }
}
