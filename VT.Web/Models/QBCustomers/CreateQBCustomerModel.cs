namespace VT.Web.Models.QBCustomers
{
    public class SystemCustomerModel
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactMobile { get; set; }

        public string QbCustomerId { get; set; }

        public bool IsActive { get; set; }
        
    }
    public class QuickbooksCustomerModel
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactMobile { get; set; }

        public string QbCustomerId { get; set; }

        public bool IsActive { get; set; }

    }
    public class CreateQBCustomerModel
    {
        public string AuthorizationToken { get; set; }
        public string jsonCustomer { get; set; }
    }

    public class SyncCustomerModel
    {
        public SystemCustomerModel SystemCustomer { get; set; }
        public QuickbooksCustomerModel QBCustomerModel { get; set; }

        public bool IsMatch { get; set; }

    }
}