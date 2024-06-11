namespace VT.QuickBooks.DTOs.Employee
{
    public class SyncEmployeeRequest
    {
        public int EmployeeId { get; set; }
        public string QbEmployeeId { get; set; }
        public int CompanyId { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public bool IsMatch { get; set; }
    }
}
