using AutoMapper;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Web.Mvc;
using VT.Services.DTOs;
using VT.Services.Interfaces;
using Webhooks.Models.DTO;

namespace VT.Web.Controllers
{
    [AllowAnonymous]
    public class WebhooksController : Controller
    {
        #region Fields

        private readonly IWebhookService _webhookServices;

        #endregion

        #region Constructor

        public WebhooksController(IWebhookService webhookServices)
        {
            _webhookServices = webhookServices;
        }

        #endregion

        #region Public Methods

        [Route("~/Webhooks/Webhook")]
        [HttpPost]
        public ActionResult Webhook()
        {
            string jsonData = null;
            var response = new WebhooksDataModel();
            if (System.Web.HttpContext.Current.Request.InputStream.CanSeek)
            {
                System.Web.HttpContext.Current.Request.InputStream.Seek(0, SeekOrigin.Begin);
                jsonData = new StreamReader(Request.InputStream).ReadToEnd();
                response = JsonConvert.DeserializeObject<WebhooksDataModel>(jsonData);
            }
            var data = Mapper.Map<WebhookNotificationRequest>(response);
            var webhookResponse = _webhookServices.WebhooksOperations(data);

            if (webhookResponse.Success)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(500);
        }

        #endregion
    }
}