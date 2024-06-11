namespace VT.Web.Models.QBCustomers
{
    public class SyncEmployeeModel
    {
        public int EmployeeId { get; set; }
        public int QbEmployeeId { get; set; }
        public int CompanyId { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}