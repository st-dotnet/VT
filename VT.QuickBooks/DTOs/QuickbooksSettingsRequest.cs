
namespace VT.QuickBooks.DTOs
{
    public class QuickbooksSettingsRequest
    {
        public int QbSettingsId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string InvoicePrefix { get; set; }
        public string DefaultPassword { get; set; }
        public string RealmId { get; set; }

        public bool ServicesSettings { get; set; }
        public bool CustomersSettings { get; set; }
        public bool EmployeesSettings { get; set; }

        public bool IsCopyWFInvoicesToQB { get; set; }
        public string AuthorizationToken { get; set; }
        public int? CompanyId { get; set; }
    }
}
