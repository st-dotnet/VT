namespace VT.Web.Models
{
    public class CompanyServiceListViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CompanyId { get; set; }
        public bool IsGpsOn { get; set; }
        public int Threshold { get; set; }
        public bool IsDeleted { get; set; }

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