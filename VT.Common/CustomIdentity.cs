using System.Security.Principal;

namespace VT.Common
{
    public class CustomIdentity : GenericIdentity
    {
        public CustomIdentity(string email, int id, string fullname, string role, int? companyId, string companyName,string imageUrl,
            bool hasMerchantAccount, bool hasGatewayCustomer, int paymentGatewayType) : base(email)
        {
            UserId = id;
            FullName = fullname;
            Role = role;
            CompanyId = companyId;
            CompanyName = companyName;
            ImageUrl = imageUrl;
            HasGatewayCustomer = hasGatewayCustomer;
            HasMerchantAccount = hasMerchantAccount;
            PaymentGateway = paymentGatewayType;
        }

        /// <summary>
        ///     Generic Identity calls username "Name" which is the same as Email in our case
        ///     This property just makes it a bit more user friendly to get the email address
        /// </summary>
        public string EmailAddress
        {
            get { return Name; }
        }

        public int UserId { get; private set; }
        public string FullName { get; private set; }
        public string Role { get; private set; }
        public int? CompanyId { get; private set; }
        public string CompanyName { get; private set; }
        public string ImageUrl { get; private set; }
        public bool HasMerchantAccount { get; private set; }
        public bool HasGatewayCustomer { get; private set; }
        public int PaymentGateway { get; private set; }
    }
}
