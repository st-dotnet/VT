using System;
using VT.Data;

namespace VT.Services.DTOs.SplashPayments
{
    public class SplashCustomerDetailResponse : BaseResponse
    {
        public string CustomerLoginId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerInactive { get; set; }
        public string CustomerAddress1 { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerZip { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerFax { get; set; }

        public string PaymentNumber { get; set; }
        public string PaymentCvv { get; set; }
        public string PaymentExpiration { get; set; }
        public string PaymentInactive { get; set; }
        public int CustomerId { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public int PMethod { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public int CompanyId { get; set; }
    }
     
}
