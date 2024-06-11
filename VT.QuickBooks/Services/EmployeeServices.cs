using System;
using System.Net;
using VT.Common;
using VT.Common.Utils;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.Employee;
using VT.QuickBooks.Interfaces;
using VT.QuickBooks.QBUtils;
using Xml2CSharp;

namespace VT.QuickBooks.Services
{
    public class EmployeeServices : IEmployee
    {
        #region Properties

        public string BaseUrl
        {
            get { return ApplicationSettings.QBBaseUrl; }
        }

        public string RealmId
        {
            get { return ApplicationSettings.QBRealmId; }
        }

        #endregion

        #region Methods

        public QuickbookBaseResponse CreateEmployee(string jsonEmployee, string authorizationTokenHeader)
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
                    string uri = string.Format("{0}/v3/company/{1}/employee", BaseUrl, RealmId);

                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = uri,
                        Method = QBUtils.HttpVerb.POST,
                        PostData = jsonEmployee,
                        Token = ApplicationSettings.GetAccessToken
                    };
                    response = client.MakeQBRequest();
                }
                if (response.Success)
                {
                    response.Success = true;
                    response.Message = "Employee has been created succesfully";
                    response.ResponseValue = response.ResponseValue;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public QuickbookBaseResponse UpdateEmployee(string jsonEmployee, string authorizationTokenHeader)
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
                    string uri = string.Format("{0}/v3/company/{1}/employee", BaseUrl, RealmId);

                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = uri,
                        Method = QBUtils.HttpVerb.POST,
                        PostData = jsonEmployee,
                        Token = ApplicationSettings.GetAccessToken
                    };
                    response = client.MakeQBRequest();
                }
                var employee = XmlUtil.Deserialize<EmployeeResponse>(response.ResponseValue);

                response.Success = true;
                response.Message = "Employee has been created succesfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public QuickbookBaseResponse DeleteEmployee(int employeeId)
        {
            throw new NotImplementedException();
        }

        public GetAllEmployees GetAllEmployees(string authorizationTokenHeader)
        {
            var employeesResponse = new GetAllEmployees();
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
                        employeesResponse.Success = false;
                        employeesResponse.Message = $"Token error : {tokenRefreshResponse.Message}";
                        return employeesResponse;
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
                    var query = "select * from employee";
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
                employeesResponse = XmlUtil.Deserialize<GetAllEmployees>(response.ResponseValue);

                employeesResponse.Success = response.Success;
                employeesResponse.Message = response.Message;
            }
            catch (Exception ex)
            {
                employeesResponse.Success = false;
                employeesResponse.Message = ex.Message;
            }
            return employeesResponse;
        }

        public GetEmployeeResponse GetEmployee(string employeeId, string authorizationTokenHeader)
        {
            var response = new QuickbookBaseResponse();
            var employee = new GetEmployeeResponse();
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
                        employee.Success = false;
                        employee.Message = $"Token error : {tokenRefreshResponse.Message}";
                        return employee;
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
                    string uri = string.Format("{0}/v3/company/{1}/employee/{2}", BaseUrl, RealmId, employeeId);
                    var client = new QBRestClient
                    {
                        ContentType = "application/json",
                        EndPoint = uri,
                        Method = QBUtils.HttpVerb.GET,
                        PostData = "",
                        Token = ApplicationSettings.GetAccessToken
                    };
                    response = client.MakeQBRequest();
                }
                employee = XmlUtil.Deserialize<GetEmployeeResponse>(response.ResponseValue);

                // Deserialized feteched emplyee data
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return employee;
        }

        #endregion
    }
}
