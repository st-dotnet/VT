using VT.QuickBooks.DTOs.Customers;


namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class UpdateCustomerRequest
    {
        public bool sparse { get; set; }
        public BillAddr BillAddr { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public QuickBooks.DTOs.Customers.PrimaryPhone PrimaryPhone { get; set; }
        public QuickBooks.DTOs.Customers.PrimaryEmailAddr PrimaryEmailAddr { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public bool sparse { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public string GivenName { get; set; }
        public string DisplayName { get; set; }
        public string FamilyName { get; set; }
        public QuickBooks.DTOs.Employee.PrimaryEmailAddr PrimaryEmailAddr { get; set; }
    }
    //public class TestRequest
    //{
    //    public bool sparse { get; set; }
    //    public string Id { get; set; }
    //    public string GivenName { get; set; }
    //    public string SyncToken { get; set; }
    //}
}