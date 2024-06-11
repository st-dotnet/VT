using RestSharp;
using System;
using VT.Common;
using System.Collections.Generic;
using RestSharp.Authenticators;
using VT.Services.DTOs;

namespace VT.Services
{
    public static class EmailComponent
    {
        public static IRestResponse Send(string subject, string to, List<string> bccs,
            string body, List<MailgunAttachment> attachments = null)
        {
            if (bccs == null) bccs = new List<string>();

            var client = new RestClient();
            client.BaseUrl = new Uri(ApplicationSettings.BaseUrlMailGun);
            client.Authenticator = new HttpBasicAuthenticator("api", ApplicationSettings.ApiKeyMailGun);

            var request = new RestRequest();
            request.AddParameter("domain", ApplicationSettings.DomainMailGun, ParameterType.UrlSegment);
            request.Resource = ApplicationSettings.ResourceMailGun;
            request.AddParameter("from", ApplicationSettings.EmailFromMailGun);
            request.AddParameter("to", to);

            foreach (var bcc in bccs)
            {
                request.AddParameter("bcc", bcc);
            }

            request.AddParameter("subject", subject);
            request.AddParameter("html", body);

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    request.AddFileBytes("attachment", attachment.Attachment, attachment.FileName, "application/pdf");
                }
            }

            request.Method = Method.POST;
            return client.Execute(request);
        }
    }
}