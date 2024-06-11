using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using VT.QuickBooks.DTOs;

namespace VT.QuickBooks.QBUtils
{
    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }


    public class QBAbstractClient
    {
        public string EndPoint { get; set; }
        public HttpVerb Method { get; set; }
        public string ContentType { get; set; }
        public string PostData { get; set; }
        public string Token { get; set; }

        public QBAbstractClient()
        {
            EndPoint = string.Empty;
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = string.Empty;
            Token = string.Empty;
        }
    }


    public class QBRestClient : QBAbstractClient
    {
        public QBRestClient() : base()
        {
        }

        public QBRestClient(string endPoint) : base()
        {
            EndPoint = endPoint;
        }

        public QBRestClient(string endPoint, HttpVerb method) : base()
        {
            EndPoint = EndPoint;
            Method = method;
        }

        public QBRestClient(string endPoint, HttpVerb method, string postData) : base()
        {
            EndPoint = endPoint;
            Method = method;
            PostData = postData;
        }
        public QBRestClient(string endPoint, HttpVerb method, string postData, string token) : base()
        {
            EndPoint = endPoint;
            Method = method;
            PostData = postData;
            Token = token;
        }

        public QuickbookBaseResponse MakeQBRequest()
        {
            return MakeQBRequest("");
        }
        public RefreshTokenResponse MakeTokenRefreshRequest()
        {
            return RefreshTokenRequest("");
        }

        // resfresh token
        public RefreshTokenResponse RefreshTokenRequest(string parameters)
        {
            var response = new RefreshTokenResponse();
            StringBuilder sb = new StringBuilder();

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);
                request.Method = Method.ToString();
                request.ContentLength = 0;
                request.ContentType = ContentType;
                request.Headers.Add("Authorization", "Basic " + Token);
                request.UseDefaultCredentials = true;
                request.PreAuthenticate = true;
                request.Credentials = CredentialCache.DefaultCredentials;

                if (!string.IsNullOrEmpty(PostData) && (Method == HttpVerb.POST || Method == HttpVerb.PUT))
                {
                    var encoding = new UTF8Encoding();
                    var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                    request.ContentLength = bytes.Length;

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(PostData);
                        streamWriter.Flush();
                    }
                }
                using (var qbResponse = (HttpWebResponse)request.GetResponse())
                {
                    if (qbResponse.StatusCode != HttpStatusCode.OK)
                    {
                        response.Success = false;
                        response.Message = string.Format("Request failed. Received HTTP {0}", qbResponse.StatusCode);
                    }
                    else
                    {   // grab the response
                        using (var responseStream = qbResponse.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (var reader = new StreamReader(responseStream))
                                {
                                    response.Success = true;
                                    response.ResponseValue = reader.ReadToEnd();
                                }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var webResponseError = new HttpWebRequestModel();
                var invalidGrantError = new GrantErrorModel();
                using (WebResponse errResponse = ex.Response)
                {
                    response.Success = false;
                    HttpWebResponse httpResponse = (HttpWebResponse)errResponse;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = errResponse.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        webResponseError = JsonConvert.DeserializeObject<HttpWebRequestModel>(text);

                        if (webResponseError.Fault == null)
                        {
                            invalidGrantError = JsonConvert.DeserializeObject<GrantErrorModel>(text);
                            response.Success = false;
                            response.Message = sb.Append(invalidGrantError.error).ToString();
                        }
                        else
                        {
                            foreach (var errText in webResponseError.Fault.Error)
                            {
                                sb.Append(errText + ", ");
                            }
                            response.Success = false;
                            response.Message = sb.ToString();
                        }
                    }
                }
            }
            return response;
        }

        // quickbooks request
        public QuickbookBaseResponse MakeQBRequest(string parameters)
        {
            var response = new QuickbookBaseResponse();
            StringBuilder sb = new StringBuilder();

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);
                request.Method = Method.ToString();
                request.ContentLength = 0;
                request.ContentType = ContentType;
                request.Headers.Add("Authorization", "Bearer " + Token);
                request.UseDefaultCredentials = true;
                request.PreAuthenticate = true;
                request.Credentials = CredentialCache.DefaultCredentials;

                if (!string.IsNullOrEmpty(PostData) && (Method == HttpVerb.POST || Method == HttpVerb.PUT))
                {
                    var encoding = new UTF8Encoding();
                    var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                    request.ContentLength = bytes.Length;

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(PostData);
                        streamWriter.Flush();
                    }
                }
                using (var qbResponse = (HttpWebResponse)request.GetResponse())
                {
                    if (qbResponse.StatusCode != HttpStatusCode.OK)
                    {
                        response.Success = false;
                        response.Message = string.Format("Request failed. Received HTTP {0}", qbResponse.StatusCode);
                    }
                    else
                    {   // grab the response
                        using (var responseStream = qbResponse.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (var reader = new StreamReader(responseStream))
                                {
                                    response.Success = true;
                                    response.ResponseValue = reader.ReadToEnd();
                                }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                response.Success = false;
                using (WebResponse errResponse = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)errResponse;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = errResponse.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        var error = JsonConvert.DeserializeObject<HttpWebRequestModel>(text);

                        foreach (var errText in error.Fault.Error)
                        {
                            sb.Append(errText.Message);
                        }
                        response.Message = sb.ToString();
                    }
                }
            }
            return response;
        }
    }
}
