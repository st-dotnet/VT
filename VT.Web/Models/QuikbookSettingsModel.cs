namespace VT.Web.Models
{
    public class QuickbooksSettingsModel
    {
        public int QbSettingsId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string InvoicePrefix { get; set; }
        public string DefaultPassword { get; set; }
        public bool IsQuickbooksIntegrated { get; set; }
        public string RealmId { get; set; }

        public bool IsCopyWFInvoicesToQB { get; set; }

        public bool ServicesSettings { get; set; }
        public bool CustomersSettings { get; set; }
        public bool EmployeesSettings { get; set; }

        public string AuthorizationToken { get; set; }
        public int? CompanyId { get; set; }

    }
}