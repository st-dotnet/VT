namespace VT.QuickBooks.DTOs.Customers
{
    public class CreateCustomerRequest
    {
        public BillAddr BillAddr { get; set; }
        public string Notes { get; set; }
        public string Title { get; set; }
        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public string FamilyName { get; set; }
        public string Suffix { get; set; }
        public string FullyQualifiedName { get; set; }
        public string CompanyName { get; set; }
        public string DisplayName { get; set; }
        public PrimaryPhone PrimaryPhone { get; set; }
        public PrimaryEmailAddr PrimaryEmailAddr { get; set; }
    }
    public class CreateQBCustomerRequest
    {
        public string AuthorizationToken { get; set; }
        public string JsonCustomer{ get; set; }
    }
}
