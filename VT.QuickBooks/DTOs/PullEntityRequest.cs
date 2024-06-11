namespace VT.QuickBooks.DTOs
{
    public class PullEntityRequest
    {
        public string AuthorizationTokenHeader { get; set; }
        public int CompanyId { get; set; }
        public string PasswordSalt { get; set; }
        public string HashedPassword { get; set; }
    }
}
