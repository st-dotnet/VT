using System;
namespace VT.Data.Entities
{
    public class QuickbookSettings
    {
        public int QuickbookSettingsId { get; set; }

        public bool ServiceSettings { get; set; }
        public bool CustomerSettings { get; set; }
        public bool EmployeeSettings { get; set; }
        public string RealmId { get; set; }

        public bool IsCopyWFInvoicesToQB { get; set; }

        public DateTime CreatedOn { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string InvoicePrefix { get; set; }
        public string DefaultPassword { get; set; }
        public int CompanyId { get; set; }

        public bool IsQuickbooksIntegrated { get; set; }

        public virtual Company Company { get; set; }
    }
}
