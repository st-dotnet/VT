﻿namespace VT.Services.DTOs
{
    public class CustomerDetailResponse : BaseResponse
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }

        public string Name { get; set; }
        public string GatewayCustomerId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public bool IsCreditCardSetup { get; set; }
        public string PostalCode { get; set; }
        public bool IsCcActive { get; set; }

        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactMobile { get; set; }
        public string QuickbookId { get; set; }
        public string ContactMiddleName { get; set; }
    }
}
