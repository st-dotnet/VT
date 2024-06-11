using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT.Services.DTOs;

namespace VT.Services.Interfaces
{
    public interface IWebhookService
    {
        BaseResponse WebhooksOperations(WebhookNotificationRequest request);
    }
}
