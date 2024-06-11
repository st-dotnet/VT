using VT.Data.Context;
using System;
using VT.QuickBooks.DTOs;
using VT.QuickBooks.Interfaces;
using VT.Data.Entities;
using System.Linq;
using VT.Common.Utils;

namespace VT.QuickBooks.Services
{
    public class QuickbookSettingsServices : IQuickbookSettings
    {
        #region Fields

        private readonly IVerifyTechContext _context;
        private readonly IEmployee _qbEmployeeServices;
        private readonly ICustomer _qbCustomerServices;
        private readonly IInvoices _qbInvoiceServices;

        #endregion

        #region Constructor

        public QuickbookSettingsServices(IVerifyTechContext context, IEmployee qbEmployeeServices
            , ICustomer qbCustomerService, IInvoices qbInvoiceService)
        {
            _context = context;
            _qbEmployeeServices = qbEmployeeServices;
            _qbCustomerServices = qbCustomerService;
            _qbInvoiceServices = qbInvoiceService;
        }

        #endregion

        #region Public Method(s)

        public QuickbookSettings GetQuickbookSettings(int companyId)
        {
            var settings = _context.QuickbookSettings.FirstOrDefault(x => x.CompanyId == companyId);
            if (settings == null) settings = new QuickbookSettings();
            return settings;
        }

        public QuickbookBaseResponse SyncQuickbookSettings(QuickbooksSettingsRequest request)
        {
            var response = new QuickbookBaseResponse();

            try
            {
                var qbSettings = request.QbSettingsId > 0 ?
               _context.QuickbookSettings.FirstOrDefault(x => x.QuickbookSettingsId == request.QbSettingsId) :
               new QuickbookSettings
               {
                   CreatedOn = DateTime.UtcNow
               };

                qbSettings.CustomerSettings = request.CustomersSettings;
                qbSettings.EmployeeSettings = request.EmployeesSettings;
                qbSettings.ServiceSettings = request.ServicesSettings;
                qbSettings.RealmId = request.RealmId;
                qbSettings.IsQuickbooksIntegrated = true;
                qbSettings.IsCopyWFInvoicesToQB = request.IsCopyWFInvoicesToQB;
                qbSettings.ClientId = request.ClientId;
                qbSettings.ClientSecret = request.ClientSecret;
                qbSettings.DefaultPassword = request.DefaultPassword;
                qbSettings.InvoicePrefix = request.InvoicePrefix;
                qbSettings.CompanyId = request.CompanyId.Value;

                if (request.QbSettingsId == 0)
                    _context.QuickbookSettings.Add(qbSettings);

                _context.SaveChanges();

                var pullEntityRequest = new PullEntityRequest();

                pullEntityRequest.AuthorizationTokenHeader = request.AuthorizationToken;
                pullEntityRequest.CompanyId = qbSettings.CompanyId;
                pullEntityRequest.PasswordSalt = PasswordUtil.GenerateSalt();
                pullEntityRequest.HashedPassword = PasswordUtil.CreatedHashedPassword(request.DefaultPassword, pullEntityRequest.PasswordSalt);

                response.Success = true;
                response.Message = "Settings have been saved successfully. " + response.Message;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        #endregion

        #region Private Method(s)

        // sync employees from quickbooks
        private QuickbookBaseResponse PullAllEmployeesFromQB(PullEntityRequest request)
        {
            var response = new QuickbookBaseResponse();

            try
            {
                // fetch employees from quikcbooks
                var qbEmployeesResponse = _qbEmployeeServices.GetAllEmployees(request.AuthorizationTokenHeader);
                var dbEmployees = _context.CompanyWorkers.ToList();

                if (!qbEmployeesResponse.Success)
                {
                    response.Message = qbEmployeesResponse.Message;
                    return response;
                }
                int count = 0;

                foreach (var employee in qbEmployeesResponse.QueryResponse.Employees)
                {
                    var isEmpExist = dbEmployees.FirstOrDefault(x => x.QBEmployeeId == employee.Id);

                    if (isEmpExist == null)
                    {
                        var qbEmployee = new CompanyWorker
                        {
                            CompanyId = request.CompanyId,
                            FirstName = employee.GivenName,
                            IsDeleted = employee.Active == "true" ? false : true,
                            LastName = employee.FamilyName,
                            QBEmployeeId = employee.Id,
                            PasswordSalt = request.PasswordSalt,
                            HashedPassword = request.HashedPassword,
                            Email = employee.PrimaryEmailAddr != null ? employee.PrimaryEmailAddr.Address : "",
                        };
                        _context.CompanyWorkers.Add(qbEmployee);
                        _context.SaveChanges();
                        count++;
                    }
                }
                response.Success = true;
                response.Message = count == 0 ? "Synchronization with Quickbooks is up to date." : $" ({count}) Employee(s) have been synchronized successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private QuickbookBaseResponse PullAllCustomersFromQB(string authorizationToken)
        {
            var response = new QuickbookBaseResponse();

            try
            {
                // pull customers from Quick Book and save in database
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        private QuickbookBaseResponse PullServicesFromQB(string authorizationToken)
        {
            var response = new QuickbookBaseResponse();

            try
            {
                // pull services from Quick Book and save in database
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        #endregion
    }
}
