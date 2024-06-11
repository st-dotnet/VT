using System;
using System.Collections.Generic;

namespace VT.QuickBooks.DTOs
{
    public class Error
    {
        public string Message { get; set; }
        public string Detail { get; set; }
        public string code { get; set; }
        public string element { get; set; }
    }

    public class Fault
    {
        public List<Error> Error { get; set; }
        public string type { get; set; }
    }

    public class HttpWebRequestModel
    {
        public Fault Fault { get; set; }
        public DateTime time { get; set; }
    }
    public class GrantErrorModel
    {
        public string error { get; set; }
    }
}
