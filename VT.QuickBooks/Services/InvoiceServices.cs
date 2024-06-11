using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT.Common;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.Interfaces;
using VT.QuickBooks.QBUtils;

namespace VT.QuickBooks.Services
{
    public class InvoiceServices : IInvoice
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

        public QuickbookBaseResponse CreateInvoice(string jsonInvoice, string authorizationToken)
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
                    isMakeCall = true;
                }

                if (isMakeCall)
                {
                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = string.Format("{0}/v3/company/{1}/invoice/", BaseUrl, RealmId),
                        Method = HttpVerb.POST,
                        PostData = jsonInvoice,
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

        #endregion
    }
}
