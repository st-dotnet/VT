using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using VT.Common;
using VT.Data.Context;
using VT.Data.Entities;
using VT.Services.Components;
using VT.Services.DTOs;
using VT.Services.DTOs.QBEntitiesRequestResponse;
using VT.Services.DTOs.SplashPayments;
using VT.Services.Interfaces;

namespace VT.Services.Services
{
    public class CompanyService : ICompanyService
    {
        #region Field(s)

        private readonly IVerifyTechContext _context;

        #endregion

        #region Constructor

        public CompanyService(IVerifyTechContext context)
        {
            _context = context;
        }

        #endregion

        #region Interface implementation

        public CompanySaveResponse Save(CompanySaveRequest request)
        {
            var response = new CompanySaveResponse();

            if (request.OrganizationId > 0) //Edit
            {
                var company = _context.Companies
                    .Include(x => x.Addresses)
                    .Include(x => x.ContactPersons)
                    .FirstOrDefault(x => x.CompanyId == request.OrganizationId);

                if (company == null)
                {
                    response.Message = "This organization does not exist";
                    return response;
                }
                company.Name = request.Name;
                //company.IsGpsOn = request.IsGpsOn;
                //company.Threshold = request.Threshold;
                var address = company.Addresses.FirstOrDefault();
                var contactPerson = company.ContactPersons.FirstOrDefault();
                //Address
                if (address == null)
                {
                    AddAddress(company, request);
                }
                else
                {
                    address.StreetAddress = request.Address;
                    address.City = request.City;
                    address.Territory = request.State;
                    address.Country = request.Country;
                    address.PostalCode = request.PostalCode;
                }
                //Contact Person
                if (contactPerson == null)
                {
                    AddContactPerson(company, request);
                }
                else
                {
                    contactPerson.Email = request.ContactEmail;
                    contactPerson.FirstName = request.ContactFirstName;
                    contactPerson.MiddleName = request.ContactMiddleName;
                    contactPerson.LastName = request.ContactLastName;
                    contactPerson.Telephone = request.ContactTelephone;
                    contactPerson.Mobile = request.ContactMobile;
                }
                if (company.ServiceFeePercentage != request.ServiceFeePercentage)
                {
                    company.ServiceFeePercentage = request.ServiceFeePercentage;

                    // if there is change in the service fee percentage then 
                    // update the existing fee inactive and create a new fee record in the splash
                    if (company.PaymentGatewayType == Data.PaymentGatewayType.Splash && !string.IsNullOrEmpty(company.MerchantJson) && !string.IsNullOrEmpty(company.FeeJson))
                    {
                        var splashMerchant = JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);
                        var gateway = new SplashPaymentComponent();

                        var existingFeeResponse = JsonConvert.DeserializeObject<SplashGatewayMerchantFeeResponse>(company.FeeJson);

                        //make existing fee inactive
                        var updateExistingFee = new CreateFeeRequest
                        {
                            // see documentation at : 
                            // https://test-portal.splashpayments.com/docs/api#feesPut for complete details

                            // (super) referrer merchant's entity id
                            ReferrerId = ApplicationSettings.SplashReferrerEntityId,
                            // (sub) merchant's entity id
                            ForEntityId = splashMerchant.response.data[0].entity.id,
                            StartDate = existingFeeResponse.response.data[0].start,
                            Amount = existingFeeResponse.response.data[0].amount,
                            Currency = existingFeeResponse.response.data[0].currency,
                            Um = existingFeeResponse.response.data[0].um,
                            Schedule = existingFeeResponse.response.data[0].schedule,
                            FeeName = String.Format("{0} INACTIVE on {1} UTC", existingFeeResponse.response.data[0].name, DateTime.UtcNow.ToString("G")),
                            ScheduleFactor = existingFeeResponse.response.data[0].scheduleFactor.ToString(),
                            Inactive = "1", // '1' means inactive and a value of '0' means active
                            Frozen = "0" // '1' means frozen and a value of '0' means not frozen
                        };
                        var feeResponse = gateway.UpdateFee(updateExistingFee, existingFeeResponse.response.data[0].id);
                        if (feeResponse.response.errors.Count == 0)
                        {
                            // company.FeeJson = JsonConvert.SerializeObject(feeResponse);
                        }

                        var percentageFee = company.ServiceFeePercentage - ApplicationSettings.SplashTransactionFee;

                        var splashFeeRquest = new CreateFeeRequest
                        {
                            // see documentation at : 
                            // https://test-portal.splashpayments.com/docs/api#feesPut for complete details

                            // (super) referrer merchant's entity id
                            ReferrerId = ApplicationSettings.SplashReferrerEntityId,
                            // (sub) merchant's entity id
                            ForEntityId = splashMerchant.response.data[0].entity.id,
                            StartDate = DateTime.UtcNow.ToString("yyyyMMdd"),
                            Amount = (percentageFee * 100).ToString(),
                            Currency = "USD",
                            Um = "1", // required 1 for percentage fee and 2 for fixed fee amount
                            Schedule = "7", // 7:Capture - the Fee triggers at the capture time of a Transaction.
                            FeeName = String.Format("{0} {1}% Fee", company.Name, percentageFee),
                            ScheduleFactor = "1", // '1' (meaning 'daily')
                            Inactive = "0", // '1' means inactive and a value of '0' means active
                            Frozen = "0" // '1' means frozen and a value of '0' means not frozen
                        };
                        var craeteFeeResponse = gateway.CreateFee(splashFeeRquest);
                        if (craeteFeeResponse.response.errors.Count == 0)
                        {
                            company.FeeJson = JsonConvert.SerializeObject(craeteFeeResponse);
                        }
                    }
                }

                _context.SaveChanges();
            }
            else //Add
            {
                var org = new Company
                {
                    PaymentGatewayType = request.PaymentGatewayType,
                    Name = request.Name,
                    ServiceFeePercentage = request.ServiceFeePercentage
                };
                AddAddress(org, request);
                AddContactPerson(org, request);

                _context.Companies.Add(org);
                _context.SaveChanges();
                response.Company = org;
            }
            response.Success = true;
            return response;
        }

        public Company GetCompany(int companyId)
        {
            return _context.Companies.FirstOrDefault(x => x.CompanyId == companyId);
        }
        // deactivate org
        public BaseResponse DeactivateOrganization(int id)
        {
            var response = new BaseResponse();
            try
            {

                var company = _context.Companies.FirstOrDefault(x => x.CompanyId == id);
                if (company == null)
                {
                    response.Success = false;
                    response.Message = "Compant doesn't exists.";
                }
                // deactivate company
                company.IsDeleted = true;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Company has been deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }
        // activate organization
        public BaseResponse ActivateOrganization(int id)
        {
            var response = new BaseResponse();
            try
            {
                var company = _context.Companies.FirstOrDefault(x => x.CompanyId == id);
                if (company == null)
                {
                    response.Success = false;
                    response.Message = "Company doesn't exists.";
                    return response;
                }
                // activate org
                company.IsDeleted = false;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Organization has been activated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public BaseResponse DeleteOrgs(List<int> ids)
        {
            var response = new BaseResponse();
            try
            {
                var companies = _context.Companies.Where(x => ids.Contains(x.CompanyId)).ToList();

                foreach (var company in companies)
                {
                    company.IsDeleted = true;
                    //company.Name = string.Format("{0}_DELETED_{1}", company.Name, DateTime.UtcNow.ToString("O"));
                }

                _context.SaveChanges();
                response.Success = true;
            }
            catch (Exception exception)
            {
                response.Message = exception.ToString();
            }
            return response;
        }

        public BaseResponse SaveImageName(ImageDetailsRequest request)
        {
            var response = new BaseResponse();
            try
            {
                var company = _context.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

                if (company == null)
                {
                    response.Success = false;
                    response.Message = "Company does't exists.";
                    return response;
                }

                company.ImageName = request.File;
                _context.SaveChanges();

                response.Success = true;
                response.Message = "Image name has been saved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }
        public OrganizationDetailResponse GetOrganizationDetail(int companyId)
        {
            OrganizationDetailResponse response = null;

            var company = _context.Companies
                .Include(x => x.Addresses)
                .Include(x => x.ContactPersons)
                .FirstOrDefault(x => x.CompanyId == companyId);

            if (company != null)
            {
                var address = company.Addresses.FirstOrDefault();
                var contactPerson = company.ContactPersons.FirstOrDefault();

                response = new OrganizationDetailResponse
                {
                    OrganizationId = company.CompanyId,
                    Name = company.Name,
                    ServiceFeePercentage = company.ServiceFeePercentage,
                    Address = address != null ? address.StreetAddress : string.Empty,
                    City = address != null ? address.City : string.Empty,
                    State = address != null ? address.Territory : string.Empty,
                    PostalCode = address != null ? address.PostalCode : string.Empty,
                    Country = address != null ? address.Country : string.Empty,
                    IsDeleted = company.IsDeleted,
                    ContactFirstName = contactPerson != null ? contactPerson.FirstName : string.Empty,
                    ContactMiddleName = contactPerson != null ? contactPerson.MiddleName : string.Empty,
                    ContactLastName = contactPerson != null ? contactPerson.LastName : string.Empty,
                    ContactEmail = contactPerson != null ? contactPerson.Email : string.Empty,
                    ContactTelephone = contactPerson != null ? contactPerson.Telephone : string.Empty,
                    ContactMobile = contactPerson != null ? contactPerson.Mobile : string.Empty,
                    PaymentGatewayType = company.PaymentGatewayType,
                    ImageUrl = company.ImageName,
                    Success = true
                };
            }
            else
            {
                response = new OrganizationDetailResponse
                {
                    Message = "Organization does not exist in the database."
                };
            }
            return response;
        }

        public CompanySaveResponse SavePreferences(CompanyPreferencesRequest request)
        {
            var response = new CompanySaveResponse();
            try
            {
                var company = _context.Companies
                   .FirstOrDefault(x => x.CompanyId == request.OrganizationId);

                if (company == null)
                {
                    response.Message = "This organization does not exist";
                    return response;
                }
                company.IsGpsOn = request.IsGpsOn;
                company.Threshold = request.Threshold;
                _context.SaveChanges();
                response.Success = true;
            }
            catch (Exception exception)
            {
                response.Message = exception.Message;
            }
            return response;
        }

        public IList<Company> GetOranizationList()
        {
            return _context.Companies.ToList();
        }

        public IList<CompanyViewResponse> GetAllCompanies()
        {
            var query = _context.Companies
                .Include(x => x.ServiceRecords)
                .Include(x => x.Customers)
                .Include(x => x.CompanyWorkers);

            var list = query.Select(x => new CompanyViewResponse
            {
                Id = x.CompanyId,
                Name = x.Name,
                Customers = x.Customers.Count(),
                Services = x.ServiceRecords.Count(),
                Users = x.CompanyWorkers.Count(),
                GatewayCustomerId = x.GatewayCustomerId,
                MerchantAccountId = x.MerchantAccountId,
                PaymentGatewayType = x.PaymentGatewayType,
                IsActive = !x.IsDeleted
            });
            return list.ToList();
        }

        public BaseResponse DeleteCompany(int companyId)
        {
            var response = new BaseResponse();

            var company = _context.Companies.FirstOrDefault(x => x.CompanyId == companyId);

            if (company == null)
            {
                response.Message = "This organization does not exist";
                return response;
            }
            else
            {
                company.Name = string.Format("{0}_DELETED_{1}", company.Name, DateTime.UtcNow.ToString("O"));
                company.IsDeleted = true;
            }

            _context.SaveChanges();
            response.Success = true;
            return response;
        }

        public BaseResponse IsOrgNameExist(string name)
        {
            var response = new BaseResponse();
            var company = _context.Companies.FirstOrDefault(x => x.Name == name);
            response.Success = company != null;
            return response;

        }

        #endregion

        #region Private Method(s)

        public void AddAddress(Company company, CompanySaveRequest request)
        {
            company.Addresses = new Collection<Address>
            {
                new Address
                {
                    StreetAddress = request.Address,
                    City = request.City,
                    Territory = request.State,
                    Country = request.Country,
                    PostalCode = request.PostalCode
                }
            };
        }

        public void AddContactPerson(Company company, CompanySaveRequest request)
        {
            company.ContactPersons = new Collection<ContactPerson>
            {
                new ContactPerson
                {
                    Email = request.ContactEmail,
                    FirstName = request.ContactFirstName,
                    MiddleName = request.ContactMiddleName,
                    LastName = request.ContactLastName,
                    Telephone = request.ContactTelephone,
                    Mobile = request.ContactMobile,
                    ContactType = ContactTypes.Office.ToString() //TODO : 
                }
            };
        } 

        #endregion
    }
}
