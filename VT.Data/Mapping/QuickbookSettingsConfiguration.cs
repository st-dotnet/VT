using System.Data.Entity.ModelConfiguration;
using VT.Data.Entities;

namespace VT.Data.Mapping
{
    internal class QuickbookSettingsConfiguration : EntityTypeConfiguration<QuickbookSettings>
    {
        public QuickbookSettingsConfiguration(string schema = "qbwfdb")
        {
            ToTable(schema + ".quickbooksettings");
            HasKey(x => x.QuickbookSettingsId);

            Property(x => x.ServiceSettings).HasColumnName("ServiceSettings").IsOptional();
            Property(x => x.CustomerSettings).HasColumnName("CustomerSettings").IsOptional();
            Property(x => x.EmployeeSettings).HasColumnName("EmployeeSettings").IsOptional();
            Property(x => x.IsCopyWFInvoicesToQB).HasColumnName("IsCopyWFInvoicesToQB").IsOptional();
            Property(x => x.CreatedOn).HasColumnName("CreatedOn").IsOptional();
            Property(x => x.IsQuickbooksIntegrated).HasColumnName("IsQuickbooksIntegrated").IsOptional();
            Property(x => x.RealmId).HasColumnName("RealmId").IsOptional();

            Property(x => x.ClientId).HasColumnName("ClientId").IsOptional();
            Property(x => x.ClientSecret).HasColumnName("ClientSecret").IsOptional();
            Property(x => x.InvoicePrefix).HasColumnName("InvoicePrefix").IsOptional();
            Property(x => x.DefaultPassword).HasColumnName("DefaultPassword").IsOptional();
            Property(x => x.CompanyId).HasColumnName("CompanyId").IsOptional();

            HasOptional(x => x.Company).WithMany(x => x.QuickbooksSettings).HasForeignKey(x => x.CompanyId);
        }
    }
}
