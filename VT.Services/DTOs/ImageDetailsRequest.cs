using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT.Services.DTOs
{
    public class ImageDetailsRequest
    {
        public int CompanyId { get; set; }
        public string File { get; set; }
    }
    public class ImageDetialsResponse
    {
        public bool Succcess { get; set; }
        public string Message { get; set; }
        public int CompanyId { get; set; }

    }
}
