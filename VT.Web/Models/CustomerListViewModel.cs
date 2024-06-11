namespace VT.Web.Models
{
    public class CustomerListViewModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public bool IsCreditCardActive { get; set; }
        public bool IsCreditCardSetup { get; set; }
        public string Telephone { get; set; }
        public string GatewayCustomerId { get; set; }
        public bool IsCcActive { get; set; }
        public bool IsDeleted { get; set; }
        public string ShowDate { get; set; }

        public string PaymentConfiguredCss
        {
            get { return IsCreditCardSetup ? "primary" : "warning"; }
        }
        public string PaymentConfiguredText
        {
            get { return !string.IsNullOrEmpty(this.GatewayCustomerId) ? "Yes" : "No"; }
        }

        public int Gateway { get; set; }

        public bool IsCompanyDeleted { get; set; }

        public bool? IsActive { get; set; }

        public string IsActiveText
        {
            get { return this.IsActive.Value ? "Active" : "Not Active"; }
        }
        public string IsActiveCss
        {
            get { return this.IsActive.Value ? "primary" : "warning"; }
        }
    }
}