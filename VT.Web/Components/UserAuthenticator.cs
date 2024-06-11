using VT.Common;
using VT.Common.Utils;
using VT.Data;
using VT.Data.Entities;
using VT.Services.DTOs;
using VT.Services.Interfaces;
using VT.Web.Interfaces;

namespace VT.Web.Components
{
    public class UserAuthenticator : IUserAuthenticator
    {
        private readonly ICompanyWorkerService _userService;

        public UserAuthenticator(ICompanyWorkerService userService)
        {
            _userService = userService;
        }

        public LoginResult AuthenticateUser(string username, string password)
        {
            // Try and get the user by email address
            var response = _userService.AuthenticateUser(new LoginRequest { Email = username });

            // User not found
            if (!response.Success)
                return new LoginResult { Success = false, Message = response.Message };

            // Wrong password

            var hashedPassword = PasswordUtil.CreatedHashedPassword(password, response.CompanyWorker.PasswordSalt);

            if (response.CompanyWorker.HashedPassword != hashedPassword)
                return new LoginResult { Success = false, Message = "The password you provided is incorrect." };

            // Got this far, all is well

            // Assign proper role
            string role = response.CompanyWorker.CompanyId == null && response.CompanyWorker.IsAdmin
                ? UserRoles.SuperAdmin.ToString()
                : (response.CompanyWorker.IsAdmin ? UserRoles.CompanyAdmin.ToString() : UserRoles.CompanyUser.ToString());

            if (response.CompanyWorker.Company != null)
            {
                string gateway = response.CompanyWorker.CompanyId == null && response.CompanyWorker.IsAdmin
                ? UserRoles.SuperAdmin.ToString()
                : (response.CompanyWorker.IsAdmin ? UserRoles.CompanyAdmin.ToString() : UserRoles.CompanyUser.ToString());
            }

            var loginResult = new LoginResult
            {
                User = response.CompanyWorker,
                Role = role,
                Gateway = (response.CompanyWorker.Company != null) ? response.CompanyWorker.Company.PaymentGatewayType :
                        PaymentGatewayType.Braintree
            };

            var hasMerchantAccount = loginResult.User.Company != null && !string.IsNullOrEmpty(loginResult.User.Company.MerchantAccountId);

            var hasGatewayCustomer = loginResult.User.Company != null && !string.IsNullOrEmpty(loginResult.User.Company.GatewayCustomerId);

           
            if (loginResult.Role == UserRoles.CompanyUser.ToString())
            {
                loginResult.Success = hasGatewayCustomer && hasMerchantAccount;
                loginResult.Message = !loginResult.Success ? "Your organization is inactive. Please contact company admin." : string.Empty;
                loginResult.RedirectToCompanyUser = true;
            }

            if (loginResult.User.Company != null && loginResult.User.Company.IsDeleted)
            {
                loginResult.Success = false;
                loginResult.Message = "The username you entered belongs to an inactive organization";
            }
            else
            {
                loginResult.Success = true;
                loginResult.Message = "Login successful";
            }
            return loginResult;
        }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public CompanyWorker User { get; set; }
        public string Message { get; set; }
        public string Role { get; set; }
        public PaymentGatewayType Gateway { get; set; }
        public bool RedirectToCompanyUser { get; set; }
    }
}