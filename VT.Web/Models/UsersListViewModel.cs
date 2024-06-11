namespace VT.Web.Models
{
    public class UsersListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string IsAdmin { get; set; }
        public bool IsCompanyDeleted { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
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