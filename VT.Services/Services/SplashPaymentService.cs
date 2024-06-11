using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using VT.Common;
using VT.Common.Utils;
using VT.Data.Context;
using VT.Data.Entities;
using VT.Services.Components;
using VT.Services.DTOs;
using VT.Services.DTOs.SplashPayments;
using VT.Services.Interfaces;

namespace VT.Services.Services
{
    public class SplashPaymentService : ISplashPaymentService
    {
        #region Field

        private readonly IVerifyTechContext _verifyTechContext;

        #endregion

        #region Constructor

        public SplashPaymentService(IVerifyTechContext verifyTechContext)
        {
            _verifyTechContext = verifyTechContext;
        }

        #endregion

        #region Interface implementation

        //Create Merchant (MerchnatJson)
        public SplashAccountResponse CreateMerchant(SplashCreateMerchantRequest request)
        {
            var response = new SplashAccountResponse();

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            try
            {
                var estDate = Convert.ToDateTime(request.Established);
                string formattedEstDate = string.Format("{0:yyyyMMdd}", estDate);

                var memberDob = Convert.ToDateTime(request.MemberDateOfBirth);
                string formattedDob = string.Format("{0:yyyyMMdd}", memberDob);

                StringBuilder sb = new StringBuilder();
                // split phonenumber;
                char[] seprators = new char[] { ' ', '.', '(', ')', '-' };
                string[] phone = request.EntityPhone.Split(seprators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in phone)
                {
                    sb.Append(item);
                }
                var entityPhone = sb.ToString();

                // clear string builder buffer array
                sb.Clear();
                string[] ssnArray = request.MemberSocialSecurityNumber.Split(seprators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in ssnArray)
                {
                    sb.Append(item);
                }
                var customSSN = sb.ToString();

                // custom ssn
                StringBuilder sb1 = new StringBuilder();

                char[] seprators1 = new char[] { ',', '.', ' ' };

                string[] annualSales = request.AnnualCCSales.Split(seprators1, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in annualSales)
                {
                    sb1.Append(item);
                }

                var customAnnualSales = (int.Parse(sb1.ToString()) * 100);
                request.AnnualCCSales = customAnnualSales.ToString();

                var annualCcSales = request.AnnualCCSales.Replace(",", string.Empty);
                int ccSales = 0;
                Int32.TryParse(annualCcSales, out ccSales);

                var createMerchantRequest = new SplashCreateMerchant
                {
                    AnnualCCSales = (ccSales * 100).ToString(),
                    Established = formattedEstDate,
                    MerchantCategoryCode = request.MerchantCategoryCode,
                    MerchantNew = "0",
                    DBA = request.DBA,
                    Status = "1",
                    TCVersion = "1",
                    Entity = new SplashCreateEntity
                    {
                        LoginId = ApplicationSettings.SplashSuperMerchantLoginId,
                        Address1 = request.EntityAddress1,
                        Address2 = request.EntityAddress2,
                        City = request.EntityCity,
                        Country = request.EntityCountry,
                        Email = request.EntityEmail,
                        Name = request.EntityName,
                        EIN = request.EntityEIN,
                        Phone = entityPhone,
                        State = request.EntityState,
                        Type = request.EntityType,
                        Website = request.EntityWebsite,
                        Zip = request.EntityZip,
                        Accounts = new List<SplashCreateAccount>()
                        {
                            new SplashCreateAccount
                            {
                                Account = new SplashAccount
                                {
                                    CardOrAccountNumber = request.CardOrAccountNumber,
                                    PaymentMethod = request.AccountsPaymentMethod,
                                    RoutingCode = request.AccountsRoutingCode
                                },
                                Primary = "1"
                            }
                        }
                    },
                    Members = new List<SplashCreateMember>()
                    {
                        new SplashCreateMember
                        {
                            Title = request.MemberTitle,
                            DateOfBirth = formattedDob,
                            DriverLicense = request.MemberDriverLicense,
                            DriverLicenseState = request.MemberDriverLicenseState,
                            Email = request.MemberEmail,
                            FirstName = request.MemberFirstName,
                            LastName = request.MemberLastName,
                            OwnerShip = request.MemberOwnerShip,
                            SocialSecurityNumber=customSSN,
                            Primary = "1",
                        }
                    }
                };

                var gateway = new SplashPaymentComponent();
                var json = gateway.CreateMerchant(createMerchantRequest);
                var splashMerchant = JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(json);


                if (splashMerchant.response.errors.Count != 0)
                {
                    response.Success = false;
                    var error = new StringBuilder();
                    foreach (var splashError in splashMerchant.response.errors)
                    {
                        error.Append(splashError.msg + "<br/>");
                    }
                    response.Message = error.ToString();
                }
                else
                {
                    response.Success = true;
                    response.Message = "Merchant has been saved successfully.";
                    company.MerchantAccountId = splashMerchant.response.data[0].id;
                    // update date of birth
                    splashMerchant.response.data[0].members[0].dob = request.MemberDateOfBirth;
                    splashMerchant.response.data[0].established = request.Established;
                    splashMerchant.response.data[0].entity.accounts[0].account.routing = request.AccountsRoutingCode;
                    splashMerchant.response.data[0].entity.accounts[0].account.number = request.CardOrAccountNumber;
                    splashMerchant.response.data[0].members[0].ssn = request.MemberSocialSecurityNumber;
                    splashMerchant.response.data[0].entity.ein = request.EntityEIN;
                    splashMerchant.response.data[0].annualCCSales = request.AnnualCCSales;

                    // serialized it and dtore in database
                    string customJson = JsonConvert.SerializeObject(splashMerchant);

                    company.MerchantJson = customJson;
                    _verifyTechContext.SaveChanges();

                    // create fees record in the splash payment gateway
                    // this is for commission

                    var percentageFee = company.ServiceFeePercentage;

                    var splashFeeRquest = new CreateFeeRequest
                    {
                        // see documentation at : 
                        // https://test-portal.splashpayments.com/docs/api#feesPut for complete details

                        // (super) referrer merchant's entity id
                        ReferrerId = ApplicationSettings.SplashReferrerEntityId,
                        // (sub) merchant's entity id
                        ForEntityId = splashMerchant.response.data[0].entity.id,
                        StartDate = DateTime.UtcNow.ToString("yyyyMMdd"),
                        Amount = (percentageFee * 100 - ApplicationSettings.SplashTransactionFee).ToString(),
                        Currency = "USD",
                        Um = "1", // required 1 for percentage fee and 2 for fixed fee amount
                        Schedule = "7", // 7:Capture - the Fee triggers at the capture time of a Transaction.
                        FeeName = String.Format("{0} {1}% Fees", company.Name, percentageFee),
                        ScheduleFactor = "1", // '1' (meaning 'daily')
                        Inactive = "0", // '1' means inactive and a value of '0' means active
                        Frozen = "0" // '1' means frozen and a value of '0' means not frozen
                    };
                    var feeResponse = gateway.CreateFee(splashFeeRquest);
                    company.FeeJson = JsonConvert.SerializeObject(feeResponse);
                    _verifyTechContext.SaveChanges();
                    response.Message = feeResponse.response.errors.Count != 0 ?
                        "Merchant has been successfully created but there was some error occured while creating fees." :
                        "Merchant creation and percentage fee record has been successfully created.";
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        // Update merchant
        public BaseResponse UpdateMerchantInfo(UpdateMerchantInfoRequest request)
        {
            var response = new BaseResponse();
            try
            {
                response.Success = true;
                response.Message = "Merchant information has been updated successfully.";

                var company = _verifyTechContext.Companies
                    .FirstOrDefault(x => x.CompanyId == request.CompanyId);
                if (company == null)
                {
                    response.Message = "Company does not exists";
                    return response;
                }

                #region Check And Update Merchant Entity

                //check and update merchant entity
                var merchantResponse =
                       JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);
                if (merchantResponse != null)
                {
                    bool parseUpdateMerchantInfo = merchantResponse.response.data[0].entity.name == request.EntityName &&
                            merchantResponse.response.data[0].entity.email == request.EntityEmail &&
                            merchantResponse.response.data[0].entity.address1 == request.EntityAddress1 &&
                              merchantResponse.response.data[0].entity.address2 == request.EntityAddress2 &&
                            merchantResponse.response.data[0].entity.phone == request.EntityPhone &&
                            merchantResponse.response.data[0].entity.city == request.EntityCity &&
                            merchantResponse.response.data[0].entity.website == request.EntityWebsite &&
                            merchantResponse.response.data[0].entity.state == request.EntityState &&
                            merchantResponse.response.data[0].entity.zip == request.EntityZip &&
                            merchantResponse.response.data[0].established == request.Established &&
                            merchantResponse.response.data[0].entity.ein == request.EntityEIN &&
                            merchantResponse.response.data[0].dba == request.DBA &&
                               merchantResponse.response.data[0].entity.type == request.EntityType &&
                            merchantResponse.response.data[0].entity.country == request.EntityCountry;

                    if (!parseUpdateMerchantInfo)
                    {
                        // update merchant entity
                        var responseMerchantEntity = UpdateMerchantEntity(request);

                        if (!responseMerchantEntity.Success)
                        {
                            response.Success = false;
                            response.Message = responseMerchantEntity.Message.ToString();
                            return response;
                        }
                    }
                }

                #endregion

                #region Check And Update Merchant Account

                // check and update merchant account
                bool parseUpdateAccountInfo =
                 merchantResponse.response.data[0].entity.accounts[0].account.number == request.CardOrAccountNumber &&
                   merchantResponse.response.data[0].entity.accounts[0].account.routing == request.AccountsRoutingCode &&
                 merchantResponse.response.data[0].entity.accounts[0].account.method == request.AccountsPaymentMethod;

                if (!parseUpdateAccountInfo)
                {
                    //update account info
                    var responseUpdateAccountInfo = UpdateMerchantAccount(request);

                    if (!responseUpdateAccountInfo.Success)
                    {
                        response.Success = false;
                        response.Message = responseUpdateAccountInfo.Message.ToString();
                        return response;
                    }
                }

                #endregion

                #region Check And Update Merchant Memeber

                // check and update memeber
                bool parseUpdateMemeberInfo = merchantResponse.response.data[0].members[0].first == request.MemberFirstName &&
                                         merchantResponse.response.data[0].members[0].last == request.MemberLastName &&
                                         merchantResponse.response.data[0].members[0].email == request.MemberEmail &&
                                         merchantResponse.response.data[0].members[0].title == request.MemberTitle &&
                                         merchantResponse.response.data[0].members[0].dob == request.MemberDateOfBirth &&
                                         merchantResponse.response.data[0].members[0].dl == request.MemberDriverLicense &&
                                         merchantResponse.response.data[0].members[0].dlstate == request.MemberDriverLicenseState &&
                                         merchantResponse.response.data[0].members[0].ownership == request.MemberOwnerShip &&
                                         merchantResponse.response.data[0].members[0].ssn == request.MemberSocialSecurityNumber &&
                                         merchantResponse.response.data[0].members[0].last == request.MemberLastName;

                if (!parseUpdateMemeberInfo)
                {
                    // update memeber info
                    var responseUpdateMemeberInfo = UpdateMerchantMember(request);

                    if (!responseUpdateMemeberInfo.Success)
                    {
                        response.Success = false;
                        response.Message = responseUpdateMemeberInfo.Message.ToString();
                        return response;
                    }
                }

                #endregion

                #region Check And Update Merchant Info

                bool updateMerchantInfo = merchantResponse.response.data[0].annualCCSales == request.AnnualCCSales;
                if (!updateMerchantInfo)
                {
                    var responseUpdateInfo = UpdateMerchantAccountInfo(request);
                    if (!responseUpdateInfo.Success)
                    {
                        response.Success = false;
                        response.Message = responseUpdateInfo.Message.ToString();
                        return response;
                    }
                }
                #endregion                

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        public BaseResponse UpdateMerchantAccountInfo(UpdateMerchantInfoRequest request)
        {
            var response = new BaseResponse();

            var estDate = Convert.ToDateTime(request.Established);
            string formattedEstDate = string.Format("{0:yyyyMMdd}", estDate);

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }
            if (!string.IsNullOrEmpty(company.MerchantJson))
            {
                try
                {
                    var merchantResponse =
                        JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);
                    if (merchantResponse != null)
                    {
                        var merchantId = merchantResponse.response.data[0].id;

                        var gateway = new SplashPaymentComponent();

                        var annualSales = request.AnnualCCSales.Replace(",", string.Empty);

                        int sales = 0;

                        Int32.TryParse(annualSales, out sales);

                        if (sales > 0)
                        {
                            var splashMerchantUpdateResponse = gateway.UpdateMerchant(new SplashMerchantUpdate
                            {
                                AnnualCCSales = (sales * 100).ToString(),
                                Established = formattedEstDate,
                                MerchantCategoryCode = request.MerchantCategoryCode
                            }, merchantId);

                            if (splashMerchantUpdateResponse.response.errors.Count != 0)
                            {
                                response.Success = false;
                                var error = new StringBuilder();
                                foreach (var splashError in splashMerchantUpdateResponse.response.errors)
                                {
                                    error.Append(splashError.msg + "<br/>");
                                }
                                response.Message = error.ToString();
                            }
                            else
                            {
                                merchantResponse.response.data[0].annualCCSales = request.AnnualCCSales;
                                merchantResponse.response.data[0].established = request.Established;
                                merchantResponse.response.data[0].mcc = request.MerchantCategoryCode;

                                company.MerchantJson = JsonConvert.SerializeObject(merchantResponse);
                                _verifyTechContext.SaveChanges();

                                response.Success = true;
                                response.IsMerchantChangedSuccessfully = true;
                                response.Message = "Merchant has been updated successfully.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //Update company customer
        public BaseResponse UpdateCompanyCustomer(UpdateSplashCustomerRequest request)
        {
            var response = new BaseResponse();

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }
            if (!string.IsNullOrEmpty(company.CustomerJson))
            {
                try
                {
                    var customerResponse = JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(company.CustomerJson);
                    if (customerResponse != null)
                    {
                        var customerId = customerResponse.response.data[0].customer.id;
                        var gateway = new SplashPaymentComponent();
                        var splashCreateCustomerResponse = gateway.UpdateCustomer(new SplashUpdateCustomerRequest
                        {
                            CompanyId = request.CompanyId,
                            CustomerFirstName = request.FirstName,
                            CustomerLastName = request.LastName,
                            CustomerEmail = request.Email
                        }, customerId);
                        if (splashCreateCustomerResponse.response.errors.Count != 0)
                        {
                            response.Success = false;
                            var error = new StringBuilder();
                            foreach (var splashError in splashCreateCustomerResponse.response.errors)
                            {
                                error.Append(splashError.msg + "<br/>");
                            }
                            response.Message = error.ToString();
                        }
                        else
                        {
                            customerResponse.response.data[0].customer.first = request.FirstName;
                            customerResponse.response.data[0].customer.last = request.LastName;
                            customerResponse.response.data[0].customer.email = request.Email;

                            company.CustomerJson = JsonConvert.SerializeObject(customerResponse);
                            _verifyTechContext.SaveChanges();

                            response.Success = true;
                            response.Message = "Customer has been updated successfully.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //Update customer customer
        public BaseResponse UpdateCustomerCustomer(UpdateSplashCustomerRequest request)
        {
            var response = new BaseResponse();

            var customerDb = _verifyTechContext.Customers.FirstOrDefault(x => x.CustomerId == request.CustomerId);

            if (customerDb == null)
            {
                response.Message = "Customer does not exists";
                return response;
            }

            if (!string.IsNullOrEmpty(customerDb.CustomerJson))
            {
                try
                {
                    var customerResponse = JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(customerDb.CustomerJson);
                    if (customerResponse != null)
                    {
                        var customerId = customerResponse.response.data[0].customer.id;

                        var gateway = new SplashPaymentComponent();

                        var splashCreateCustomerResponse = gateway.UpdateCustomer(new SplashUpdateCustomerRequest
                        {
                            CompanyId = request.CompanyId,
                            CustomerFirstName = request.FirstName,
                            CustomerLastName = request.LastName,
                            CustomerEmail = request.Email
                        }, customerId);

                        if (splashCreateCustomerResponse.response.errors.Count != 0)
                        {
                            response.Success = false;
                            var error = new StringBuilder();
                            foreach (var splashError in splashCreateCustomerResponse.response.errors)
                            {
                                error.Append(splashError.msg + "<br/>");
                            }
                            response.Message = error.ToString();
                        }
                        else
                        {
                            customerResponse.response.data[0].customer.first = request.FirstName;
                            customerResponse.response.data[0].customer.last = request.LastName;
                            customerResponse.response.data[0].customer.email = request.Email;

                            customerDb.CustomerJson = JsonConvert.SerializeObject(customerResponse);
                            _verifyTechContext.SaveChanges();

                            response.Success = true;
                            response.Message = "Customer has been updated successfully.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //update entity
        public BaseResponse UpdateMerchantEntity(UpdateMerchantInfoRequest request)
        {
            var response = new BaseResponse();

            var estDate = Convert.ToDateTime(request.Established);
            string formattedEstDate = string.Format("{0:yyyyMMdd}", estDate);

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            if (!string.IsNullOrEmpty(company.MerchantJson))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    // split phonenumber;
                    char[] seprators = new char[] { ' ', '.', '(', ')', '-' };
                    string[] phone = request.EntityPhone.Split(seprators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in phone)
                    {
                        sb.Append(item);
                    }
                    var entityPhone = sb.ToString();

                    var merchantResponse = JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);
                    if (merchantResponse != null)
                    {
                        var entityId = merchantResponse.response.data[0].entity.id;
                        var gateway = new SplashPaymentComponent();
                        var splashMerchantEntityUpdateResponse = gateway.UpdateEntity(new SplashUpdateEntity
                        {
                            LoginId = ApplicationSettings.SplashSuperMerchantLoginId,
                            Address1 = request.EntityAddress1,
                            Address2=request.EntityAddress2,
                            City = request.EntityCity,
                            Country = "USA",
                            Email = request.EntityEmail,
                            EIN = request.EntityEIN,
                            Name = request.EntityName,
                            Phone = entityPhone,
                            Established = formattedEstDate,
                            State = request.EntityState,
                            Type = request.EntityType,
                            Website = request.EntityWebsite,
                            Zip = request.EntityZip
                        }, entityId);

                        if (splashMerchantEntityUpdateResponse.response.errors.Count != 0)
                        {
                            response.Success = false;
                            var error = new StringBuilder();
                            foreach (var splashError in splashMerchantEntityUpdateResponse.response.errors)
                            {
                                error.Append(splashError.msg + "<br/>");
                            }
                            response.Message = error.ToString();
                        }
                        else
                        {
                            merchantResponse.response.data[0].entity.address1 = request.EntityAddress1;
                            merchantResponse.response.data[0].entity.address2 = request.EntityAddress2;
                            merchantResponse.response.data[0].entity.city = request.EntityCity;
                            merchantResponse.response.data[0].entity.country = request.EntityCountry;
                            merchantResponse.response.data[0].entity.email = request.EntityEmail;
                            merchantResponse.response.data[0].entity.name = request.EntityName;
                            merchantResponse.response.data[0].entity.phone = request.EntityPhone;
                            merchantResponse.response.data[0].entity.state = request.EntityState;
                            merchantResponse.response.data[0].entity.website = request.EntityWebsite;
                            merchantResponse.response.data[0].entity.zip = request.EntityZip;
                            merchantResponse.response.data[0].established = request.Established;
                            merchantResponse.response.data[0].dba = request.DBA;
                            merchantResponse.response.data[0].entity.type = request.EntityType;
                            merchantResponse.response.data[0].mcc = request.MerchantCategoryCode;
                            merchantResponse.response.data[0].entity.ein = request.EntityEIN;
                            company.MerchantJson = JsonConvert.SerializeObject(merchantResponse);
                            _verifyTechContext.SaveChanges();

                            response.Success = true;
                            response.IsMerchantChangedSuccessfully = true;
                            response.Message = "Entity has been updated successfully.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //Update Member
        public BaseResponse UpdateMerchantMember(UpdateMerchantInfoRequest request)
        {
            var response = new BaseResponse();

            var memberDob = Convert.ToDateTime(request.MemberDateOfBirth);
            string formattedDob = string.Format("{0:yyyyMMdd}", memberDob);

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            if (!string.IsNullOrEmpty(company.MerchantJson))
            {
                try
                {
                    var merchantResponse =
                        JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);
                    if (merchantResponse != null)
                    {

                        StringBuilder sb = new StringBuilder();
                        // clear string builder buffer array
                        char[] seprators = new char[] { ' ', '.', '(', ')', '-' };

                        string[] ssnArray = request.MemberSocialSecurityNumber.Split(seprators, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in ssnArray)
                        {
                            sb.Append(item);
                        }
                        var customSSN = sb.ToString();

                        var merchantId = merchantResponse.response.data[0].id;
                        var memberId = merchantResponse.response.data[0].members[0].id;

                        var gateway = new SplashPaymentComponent();

                        var splashMerchantMemberUpdateResponse = gateway.UpdateMember(new SplashUpdateMember
                        {
                            Title = request.MemberTitle,
                            DateOfBirth = formattedDob,
                            Dl = request.MemberDriverLicense,
                            DlState = request.MemberDriverLicenseState,
                            FirstName = request.MemberFirstName,
                            LastName = request.MemberLastName,
                            Email = request.MemberEmail,
                            Ownership = request.MemberOwnerShip,
                            Ssn = customSSN,
                            Primary = "1",
                            MerchantId = merchantId
                        }, memberId);

                        if (splashMerchantMemberUpdateResponse.response.errors.Count != 0)
                        {
                            response.Success = false;
                            var error = new StringBuilder();
                            foreach (var splashError in splashMerchantMemberUpdateResponse.response.errors)
                            {
                                error.Append(splashError.msg + "<br/>");
                            }
                            response.Message = error.ToString();
                        }
                        else
                        {

                            var ownership = int.Parse(request.MemberOwnerShip);
                            merchantResponse.response.data[0].members[0].title = request.MemberTitle;
                            merchantResponse.response.data[0].members[0].first = request.MemberFirstName;
                            merchantResponse.response.data[0].members[0].last = request.MemberLastName;
                            merchantResponse.response.data[0].members[0].dob = request.MemberDateOfBirth;
                            merchantResponse.response.data[0].members[0].dl = request.MemberDriverLicense;
                            merchantResponse.response.data[0].members[0].dlstate = request.MemberDriverLicenseState;
                            merchantResponse.response.data[0].members[0].ownership = (ownership * 100).ToString();
                            merchantResponse.response.data[0].members[0].ssn = request.MemberSocialSecurityNumber;
                            merchantResponse.response.data[0].members[0].email = request.MemberEmail;

                            company.MerchantJson = JsonConvert.SerializeObject(merchantResponse);
                            _verifyTechContext.SaveChanges();

                            response.Success = true;
                            response.IsMerchantChangedSuccessfully = true;
                            response.Message = "Member has been updated successfully.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //Update merchant account
        public BaseResponse UpdateMerchantAccount(UpdateMerchantInfoRequest request)
        {
            var response = new BaseResponse();

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);
            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            if (!string.IsNullOrEmpty(company.MerchantJson))
            {
                try
                {
                    var merchantResponse = JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);
                    if (merchantResponse != null)
                    {
                        var accountId = merchantResponse.response.data[0].entity.accounts[0].id;
                        var gateway = new SplashPaymentComponent();

                        var splashMerchantAccountUpdateResponse = gateway.UpdateMerchantAccount(new SplashUpdateAccount
                        {
                            AccountCardOrAccountNumber = request.CardOrAccountNumber,
                            AccountRoutingCode = request.AccountsRoutingCode,
                            AccountPaymentMethod = request.AccountsPaymentMethod,
                            Primary = "1"
                        }, accountId);

                        if (splashMerchantAccountUpdateResponse.response.errors.Count != 0)
                        {
                            response.Success = false;
                            var error = new StringBuilder();
                            foreach (var splashError in splashMerchantAccountUpdateResponse.response.errors)
                            {
                                error.Append(splashError.msg + "<br/>");
                            }
                            response.Message = error.ToString();
                        }
                        else
                        {
                            merchantResponse.response.data[0].entity.accounts[0].account.number =
                                request.CardOrAccountNumber;
                            merchantResponse.response.data[0].entity.accounts[0].account.method =
                                request.AccountsPaymentMethod;
                            merchantResponse.response.data[0].entity.accounts[0].account.routing =
                                request.AccountsRoutingCode;

                            company.MerchantJson = JsonConvert.SerializeObject(merchantResponse);
                            _verifyTechContext.SaveChanges();

                            response.Success = true;
                            response.IsMerchantChangedSuccessfully = true;
                            response.Message = "Account has been updated successfully.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //Get merchant detail
        public SplashGetMerchantResponse GetMerchantDetail(int companyId)
        {
            var response = new SplashGetMerchantResponse();

            //check company
            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == companyId);

            if (company == null)
            {
                response.Message = "Company does not exists in the system.";
                return response;
            }

            response.CompanyId = companyId;

            if (!string.IsNullOrEmpty(company.MerchantJson))
            {
                try
                {
                    var merchantResponse =
                        JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);
                    if (merchantResponse != null)
                    {
                        //Merchant
                        response.AnnualCCSales = merchantResponse.response.data[0].annualCCSales;
                        response.MerchantCategoryCode = merchantResponse.response.data[0].mcc;
                        response.Established = merchantResponse.response.data[0].established;
                        response.DBA = merchantResponse.response.data[0].dba;

                        //Member
                        response.MemberTitle = merchantResponse.response.data[0].members[0].title;
                        response.MemberDateOfBirth = merchantResponse.response.data[0].members[0].dob;
                        response.MemberDriverLicense = merchantResponse.response.data[0].members[0].dl;
                        response.MemberDriverLicenseState = merchantResponse.response.data[0].members[0].dlstate;
                        response.MemberEmail = merchantResponse.response.data[0].members[0].email;
                        response.MemberFirstName = merchantResponse.response.data[0].members[0].first;
                        response.MemberLastName = merchantResponse.response.data[0].members[0].last;
                        response.MemberOwnerShip = (int.Parse(merchantResponse.response.data[0].members[0].ownership) / 100).ToString();
                        response.MemberSocialSecurityNumber = merchantResponse.response.data[0].members[0].ssn;

                        //Entity
                        response.EntityName = merchantResponse.response.data[0].entity.name;
                        response.EntityAddress1 = merchantResponse.response.data[0].entity.address1;

                        response.EntityAddress2 = merchantResponse.response.data[0].entity.address2;
                        response.EntityCity = merchantResponse.response.data[0].entity.city;
                        response.EntityState = merchantResponse.response.data[0].entity.state;
                        response.EntityEIN = merchantResponse.response.data[0].entity.ein;
                        response.EntityCountry = merchantResponse.response.data[0].entity.country;
                        response.EntityEmail = merchantResponse.response.data[0].entity.email;
                        response.EntityType = merchantResponse.response.data[0].entity.type;
                        response.EntityPhone = merchantResponse.response.data[0].entity.phone;
                        response.EntityWebsite = merchantResponse.response.data[0].entity.website;
                        response.EntityZip = merchantResponse.response.data[0].entity.zip;

                        //Account
                        response.AccountsPaymentMethod = merchantResponse.response.data[0].entity.accounts[0].account.method;
                        response.CardOrAccountNumber =
                            merchantResponse.response.data[0].entity.accounts[0].account.number;
                        response.AccountsRoutingCode =
                            merchantResponse.response.data[0].entity.accounts[0].account.routing;

                        response.Success = true;
                    }
                }
                catch (Exception ex)
                {
                    response.Message = string.Format("Company [merchant json] is not valid. Extra error info: {0}",
                        ex.ToString());
                    return response;
                }
            }

            return response;
        }

        public SplashCustomerDetailResponse GetCompanyCcDetail(int companyId)
        {
            var response = new SplashCustomerDetailResponse();

            //check company
            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == companyId);

            if (company == null)
            {
                response.Message = "Company does not exists in the system.";
                return response;
            }

            if (!string.IsNullOrEmpty(company.CustomerJson))
            {
                try
                {
                    var customerResponse =
                        JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(company.CustomerJson);
                    if (customerResponse != null)
                    {
                        response.CustomerFirstName = customerResponse.response.data[0].customer.first;
                        response.CustomerLastName = customerResponse.response.data[0].customer.last;
                        response.CustomerEmail = customerResponse.response.data[0].customer.email;
                        response.Month = customerResponse.response.data[0].expiration.Substring(0, 2);
                        response.Year = customerResponse.response.data[0].expiration.Substring(2, 2);

                        response.CompanyId = companyId;

                        var number = customerResponse.response.data[0].payment.number;
                        number = number.Length > 4 ? number.Substring(number.Length - 4) : number;
                        response.PaymentNumber = "XXXX" + number;
                        response.PaymentCvv = ".....";
                        response.PaymentExpiration = customerResponse.response.data[0].expiration;
                        response.PaymentInactive = customerResponse.response.data[0].inactive.ToString();
                        response.PMethod = customerResponse.response.data[0].payment.method;
                        response.Success = true;
                    }
                }
                catch (Exception ex)
                {
                    response.Message = string.Format("Company [customer json] is not valid. Extra error info: {0}",
                        ex.ToString());
                    return response;
                }
            }
            else
            {
                var contactPerson = company.ContactPersons.FirstOrDefault();
                if (contactPerson != null)
                {
                    response.CustomerFirstName = contactPerson.FirstName;
                    response.CustomerLastName = contactPerson.LastName;
                    response.CustomerEmail = contactPerson.Email;
                    response.CompanyId = companyId;
                    response.Success = true;
                }
            }
            return response;
        }

        public SplashCustomerDetailResponse GetCustomerCcDetail(int? customerId)
        {
            var response = new SplashCustomerDetailResponse();

            //check customer
            var customerDb = _verifyTechContext.Customers
                .Include(x => x.ContactPersons)
                .FirstOrDefault(x => x.CustomerId == customerId);

            if (customerDb == null)
            {
                response.Message = "customer does not exists in the system.";
                return response;
            }

            if (!string.IsNullOrEmpty(customerDb.CustomerJson))
            {
                try
                {
                    var customerResponse =
                        JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(customerDb.CustomerJson);
                    if (customerResponse != null)
                    {
                        response.CustomerFirstName = customerResponse.response.data[0].customer.first;
                        response.CustomerLastName = customerResponse.response.data[0].customer.last;
                        response.CustomerEmail = customerResponse.response.data[0].customer.email;

                        var number = customerResponse.response.data[0].payment.number;

                        number = number.Length > 4 ? number.Substring(number.Length - 4) : number;
                        response.Month = customerResponse.response.data[0].expiration.Substring(0, 2);
                        response.Year = customerResponse.response.data[0].expiration.Substring(2, 2);
                        response.PaymentNumber = "XXXX" + number;
                        response.PaymentCvv = ".....";
                        response.PaymentExpiration = customerResponse.response.data[0].expiration;
                        response.PaymentInactive = customerResponse.response.data[0].inactive.ToString();
                        response.PMethod = customerResponse.response.data[0].payment.method;
                    }
                }
                catch (Exception ex)
                {
                    response.Message = string.Format("Company [customer json] is not valid. Extra error info: {0}",
                        ex.ToString());
                    return response;
                }
            }
            else
            {
                if (customerDb.ContactPersons.Any())
                {
                    var contactPerson = customerDb.ContactPersons.FirstOrDefault();
                    if (contactPerson != null)
                    {
                        response.CustomerFirstName = contactPerson.FirstName;
                        response.CustomerLastName = contactPerson.LastName;
                        response.CustomerEmail = contactPerson.Email;
                        response.CustomerId = customerId.Value;
                    }
                }
            }
            response.Success = true;
            response.CompanyId = customerDb.CompanyId;
            return response;
        }

        //Get merchant
        public SplashGatewayMerchantResponse GetMerchant(int companyId)
        {
            var response = new SplashGatewayMerchantResponse();

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == companyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }
            else
            {
                response.Success = true;
                response.Message = "Merchant successfully fetched.";
            }
            return response;
        }

        //Get account
        public SplashGetMerchantResponse GetMember(int companyId)
        {
            var response = new SplashGetMerchantResponse();

            //check company
            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == companyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            var merchantInfo = JsonConvert.DeserializeObject<MerchantInfo>(company.MerchantAccountId);

            if (merchantInfo == null)
            {
                response.Message = "Merchant Account information is not available";
                return response;
            }

            response.Success = true;

            var response1 = new SplashAccountResponse();

            var client = new RestClient
            {
                EndPoint = string.Format("{0}{1}/{2}", ApplicationSettings.SplashApiUrl, "accounts", merchantInfo.AccountId),
                Method = HttpVerb.GET
            };

            var json = client.MakeRequest();

            return response;
        }

        //Get account
        public SplashGetMerchantResponse GetAccount(int companyId)
        {
            var response = new SplashGetMerchantResponse();

            //check company
            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == companyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            var merchantInfo = JsonConvert.DeserializeObject<MerchantInfo>(company.MerchantAccountId);

            if (merchantInfo == null)
            {
                response.Message = "Merchant Account information is not available";
                return response;
            }

            response.Success = true;

            var response1 = new SplashAccountResponse();

            var client = new RestClient
            {
                EndPoint = string.Format("{0}{1}/{2}", ApplicationSettings.SplashApiUrl, "accounts", merchantInfo.AccountId),
                Method = HttpVerb.GET
            };

            var json = client.MakeRequest();

            return response;
        }

        //Get entity
        public SplashGetMerchantResponse GetEntity(int companyId)
        {
            var response = new SplashGetMerchantResponse();
            var entity = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == companyId);
            return response;
        }

        public SplashTransactionResult Transaction(SplashTransactionRequest request)
        {
            var response = new SplashTransactionResult();
            var amount = (request.Amount * 100).ToString(); //usd amount converted to cents

            var customerResponse =
                        JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(request.CustomerJson);

            if (customerResponse == null)
            {
                response.Message = "Customer Json is not valid.";
                return response;
            }

            var token = customerResponse.response.data[0].token;

            var gateway = new SplashPaymentComponent();
            var transactionResponse = gateway.MakeTransaction(new SplashTransaction
            {
                MerchantId = request.MerchantId,
                Origin = "2",
                Token = token,
                Type = "1",
                Total = amount
            });

            if (transactionResponse.response.errors.Count != 0)
            {
                response.Success = false;
                var error = new StringBuilder();
                foreach (var splashError in transactionResponse.response.errors)
                {
                    error.Append(splashError.msg + "<br/>");
                }
                response.Message = error.ToString();
            }
            else
            {
                response.Success = true;
                response.TransactionId = transactionResponse.response.data[0].id;
                response.Message = "Transaction Succeeded.";
            }

            return response;
        }

        //Disable comapny customer credit card 
        public BaseResponse DisableCompanyCreditCard(int? companyId, int? customerId)
        {
            var response = new BaseResponse();

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == companyId.Value);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            var customerJsonObj = JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(company.CustomerJson);

            // it's token id and not the actual token
            var id = customerJsonObj.response.data[0].id;

            var gateway = new SplashPaymentComponent();

            var splashPaymentDetailResponse = gateway.DisableCustomerCreditCard(new DisableCreditCard
            {
                Inactive = "1",
            }, id);

            if (splashPaymentDetailResponse.response.errors.Count != 0)
            {
                response.Success = false;
                var error = new StringBuilder();
                foreach (var splashError in splashPaymentDetailResponse.response.errors)
                {
                    error.Append(splashError.msg + "<br/>");
                }
                response.Message = error.ToString();
            }
            else
            {
                response.Success = true;
                response.Message = "Payment information has been successfully deleted.";
                customerJsonObj.response.data[0].inactive = 1;
                company.CustomerJson = JsonConvert.SerializeObject(customerJsonObj);
                company.GatewayCustomerId = null;
                company.CustomerJson = null;
                _verifyTechContext.SaveChanges();
            }
            return response;
        }

        //Disable customer customer credit card
        public BaseResponse DisableCustomerCreditCard(int? companyId, int? customerId)
        {
            var response = new BaseResponse();

            var customerCc = _verifyTechContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);

            if (customerCc == null)
            {
                response.Message = "Customer does not exists";
                return response;
            }
            var customerJsonObj = JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(customerCc.CustomerJson);

            // it's token id and not the actual token
            var id = customerJsonObj.response.data[0].id;

            var gateway = new SplashPaymentComponent();

            var splashPaymentDetailResponse = gateway.DisableCustomerCreditCard(new DisableCreditCard
            {
                Inactive = "1",
            }, id);

            if (splashPaymentDetailResponse.response.errors.Count != 0)
            {
                response.Success = false;
                var error = new StringBuilder();
                foreach (var splashError in splashPaymentDetailResponse.response.errors)
                {
                    error.Append(splashError.msg + "<br/>");
                }
                response.Message = error.ToString();
            }
            else
            {
                response.Success = true;
                response.Message = "Payment information has been successfully deleted.";
                customerCc.IsCcActive = false;
                customerCc.CustomerJson = null;

                _verifyTechContext.SaveChanges();
            }
            return response;
        }

        //Add company credit card
        public SplashAccountResponse AddCompanyCreditCard(AddCustomerCreditCardRequest request)
        {
            var response = new SplashAccountResponse();

            var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

            if (company == null)
            {
                response.Message = "Company does not exists";
                return response;
            }

            if (!string.IsNullOrEmpty(company.CustomerJson))
            {
                try
                {
                    var customerResponse = JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(company.CustomerJson);

                    if (customerResponse != null)
                    {
                        var customerId = customerResponse.response.data[0].customer.id;

                        var gateway = new SplashPaymentComponent();

                        var splashAddPaymentDetailResponse = gateway.AddPaymentCreditCard(new AddCustomerCreditCard
                        {
                            Customer = customerId,
                            Inactive = "0",
                            Payment = new CustomerPaymentRequest
                            {
                                Method = request.PaymentMethod,
                                Number = request.PaymentNumber,
                                Expiration = request.PaymentExpiration,
                                Cvv = request.PaymentCvv
                            }
                        });

                        if (splashAddPaymentDetailResponse.response.errors.Count != 0)
                        {
                            response.Success = false;
                            var error = new StringBuilder();
                            foreach (var splashError in splashAddPaymentDetailResponse.response.errors)
                            {
                                error.Append(splashError.msg + "<br/>");
                            }
                            response.Message = error.ToString();
                        }
                        else
                        {
                            customerResponse.response.data[0].payment.number = request.PaymentNumber;
                            customerResponse.response.data[0].payment.method = Convert.ToInt32(request.PaymentMethod);
                            customerResponse.response.data[0].expiration = request.PaymentExpiration;
                            customerResponse.response.data[0].inactive = 0;
                            company.CustomerJson = JsonConvert.SerializeObject(customerResponse);

                            _verifyTechContext.SaveChanges();

                            response.Success = true;
                            response.Message = "Company card has been successfully added.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //Add customer credit card
        public SplashAccountResponse AddCustomerCreditCard(AddCustomerCreditCardRequest request)
        {
            var response = new SplashAccountResponse();

            var customer = _verifyTechContext.Customers.FirstOrDefault(x => x.CustomerId == request.CustomerId);

            if (customer == null)
            {
                response.Message = "Customer does not exists";
                return response;
            }

            if (!string.IsNullOrEmpty(customer.CustomerJson))
            {
                try
                {
                    var customerResponse = JsonConvert.DeserializeObject<SplashCreateCustomerResponseObject>(customer.CustomerJson);

                    if (customerResponse != null)
                    {
                        var customerId = customerResponse.response.data[0].customer.id;

                        var gateway = new SplashPaymentComponent();

                        var splashAddPaymentDetailResponse = gateway.AddPaymentCreditCard(new AddCustomerCreditCard
                        {
                            Customer = customerId,
                            Inactive = "0",
                            Payment = new CustomerPaymentRequest
                            {
                                Method = request.PaymentMethod,
                                Number = request.PaymentNumber,
                                Expiration = request.PaymentExpiration,
                                Cvv = request.PaymentCvv
                            }
                        });

                        if (splashAddPaymentDetailResponse.response.errors.Count != 0)
                        {
                            response.Success = false;
                            var error = new StringBuilder();
                            foreach (var splashError in splashAddPaymentDetailResponse.response.errors)
                            {
                                error.Append(splashError.msg + "<br/>");
                            }
                            response.Message = error.ToString();
                        }
                        else
                        {
                            customerResponse.response.data[0].payment.number = request.PaymentNumber;
                            customerResponse.response.data[0].payment.method = Convert.ToInt32(request.PaymentMethod);
                            customerResponse.response.data[0].expiration = request.PaymentExpiration;
                            customerResponse.response.data[0].inactive = 0;
                            customer.IsCcActive = true;
                            customer.CustomerJson = JsonConvert.SerializeObject(customerResponse);

                            _verifyTechContext.SaveChanges();

                            response.Success = true;
                            response.Message = "Customer card has been successfully added.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Message = ex.ToString();
                }
            }
            return response;
        }

        //Update entity
        public SplashAccountResponse UpdateEntity(SplashCreateEntityRequest request, string entityId)
        {
            var response = new SplashAccountResponse();

            try
            {
                var entity = new SplashCreateEntity
                {
                    LoginId = request.LoginId,
                    Address1 = request.Address1,
                    City = request.City,
                    Country = request.Country,
                    Email = request.Email,
                    EIN = request.EmployerId,
                    Name = request.Name,
                    Phone = request.Phone,
                    State = request.State,
                    Type = request.Type,
                    Website = request.Website,
                    Zip = request.Zip
                };
                response.Message = "Entity has been updated successfully.";
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        //Create fee
        public SplashAccountResponse CreateFee(CreateFeeRequest request)
        {
            var response = new SplashAccountResponse();

            try
            {
                var fee = new CreateFee
                {
                    ReferrerId = request.ReferrerId,
                    ForEntityId = request.ForEntityId,
                    StartDate = request.StartDate,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Um = request.Um,
                    Schedule = request.Schedule,
                    FeeName = request.FeeName
                };
                response.Message = "Fee has been created successfully.";
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        //Delete fee
        public SplashAccountResponse DeleteFee(string feeId)
        {
            var response = new SplashAccountResponse();

            var client = new RestClient();
            client.EndPoint = string.Format("{0}{1}/{2}", ApplicationSettings.SplashApiUrl, "fees", feeId);
            client.Method = HttpVerb.DELETE;

            var json = client.MakeRequest();
            return response;
        }

        //Create plan
        public SplashAccountResponse CreatePlan(SplashCreatePlanForMerchantRequest request)
        {
            var response = new SplashAccountResponse();
            try
            {
                var plan = new SplashCreatePlanForMerchant
                {
                    MerchantId = request.MerchantId,
                    Amount = request.Amount,
                    Description = request.Description,
                    Name = request.Name,
                    Schedule = request.Schedule
                };
                response.Message = "Plan has been created successfully.";
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        //Update merchant
        public SplashAccountResponse UpdateMerchant(SplashCreateMerchantRequest request)
        {
            var response = new SplashAccountResponse();
            try
            {
                // Update merchant
                var requestUpdate = new SplashCreateMerchant
                {
                    Established = request.Established,
                    MerchantNew = request.MerchantNew,
                    AnnualCCSales = request.AnnualCCSales,
                    TCVersion = request.TCVersion,
                    Status = request.Status,
                    Entity = new SplashCreateEntity
                    {
                        LoginId = request.EntityLoginId
                    },
                };
                response.Success = true;
                response.Message = "Merchant has been updated successfully.";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        //Create company customer
        public SplashAccountResponse CreateCcCustomerForCompany(SplashCustomerCreateRequest request)
        {
            var response = new SplashAccountResponse();

            var createCustomer = _verifyTechContext.Companies
                .FirstOrDefault(x => x.CompanyId == request.CompanyId);

            if (createCustomer == null)
            {
                response.Message = "This customer does not exist in the database.";
                return response;
            }
            try
            {
                var requestCreateCustomer = new CreateCustomerWithPaymentToken
                {
                    Customer = new SplashCreateCustomer
                    {
                        LoginId = ApplicationSettings.SplashSuperMerchantLoginId,
                        FirstName = request.CustomerFirstName,
                        LastName = request.CustomerLastName,
                        Email = request.CustomerEmail,
                        Inactive = "0"
                    },
                    Payment = new CustomerPaymentRequest
                    {
                        Method = request.PaymentMethod,
                        Number = request.PaymentNumber,
                        Expiration = request.PaymentExpiration,
                        Cvv = request.PaymentCvv
                    }
                };

                var gateway = new SplashPaymentComponent();
                var splashGatewayCustomerResponse = gateway.CreateCustomer(requestCreateCustomer);

                if (splashGatewayCustomerResponse.response.errors.Count != 0)
                {
                    response.Success = false;
                    var error = new StringBuilder();
                    foreach (var splashError in splashGatewayCustomerResponse.response.errors)
                    {
                        error.Append(splashError.msg + "<br/>");
                    }
                    response.Message = error.ToString();
                }
                else
                {
                    createCustomer.GatewayCustomerId = splashGatewayCustomerResponse.response.data[0].customer.id;
                    createCustomer.CustomerJson = JsonConvert.SerializeObject(splashGatewayCustomerResponse);

                    _verifyTechContext.SaveChanges();
                    response.Success = true;
                    response.Message = "Customer has been successfully saved.";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.ToString();
            }
            return response;
        }

        public SplashAccountResponse CreateCcCustomerForCustomer(SplashCustomerCreateRequest request)
        {
            var response = new SplashAccountResponse();

            var customerDb = _verifyTechContext.Customers.FirstOrDefault(x => x.CustomerId == request.CustomerId);

            if (customerDb == null)
            {
                response.Message = "This customer does not exist in the database.";
                return response;
            }
            if (customerDb.ExpireAt != null && customerDb.ExpireAt < DateTime.Now)
            {
                customerDb.ExpireAt = null;
                _verifyTechContext.SaveChanges();

                response.Message = "The link to set your payment information has already been expired.";
                return response;
            }
            try
            {
                var company = _verifyTechContext.Companies.FirstOrDefault(x => x.CompanyId == request.CompanyId);

                if (company == null)
                {
                    response.Message = string.Format("This company : {0} does not exist in the database.", request.CompanyId);
                    return response;
                }
                var splashMerchant = JsonConvert.DeserializeObject<SplashGatewayMerchantResponse>(company.MerchantJson);

                if (splashMerchant == null)
                {
                    response.Message = string.Format("MerchantJson of company : {0}  is not valid.", request.CompanyId);
                    return response;
                }

                var requestCreateCustomer = new CreateCustomerWithPaymentToken
                {
                    Customer = new SplashCreateCustomer
                    {
                        LoginId = splashMerchant.response.data[0].entity.login,
                        FirstName = request.CustomerFirstName,
                        LastName = request.CustomerLastName,
                        Email = request.CustomerEmail,
                        Inactive = "0"
                    },
                    Payment = new CustomerPaymentRequest
                    {
                        Method = request.PaymentMethod,
                        Number = request.PaymentNumber,
                        Expiration = request.PaymentExpiration,
                        Cvv = request.PaymentCvv
                    }
                };

                var gateway = new SplashPaymentComponent();
                var splashGatewayCustomerResponse = gateway.CreateCustomer(requestCreateCustomer);

                if (splashGatewayCustomerResponse.response.errors.Count != 0)
                {
                    response.Success = false;
                    var error = new StringBuilder();
                    foreach (var splashError in splashGatewayCustomerResponse.response.errors)
                    {
                        error.Append(splashError.msg + "<br/>");
                    }
                    response.Message = error.ToString();
                    response.InternalError = true;
                }
                else
                {
                    customerDb.GatewayCustomerId = splashGatewayCustomerResponse.response.data[0].customer.id;
                    customerDb.CustomerJson = JsonConvert.SerializeObject(splashGatewayCustomerResponse);
                    customerDb.IsCcActive = true;

                    if (customerDb.ExpireAt != null)
                    {
                        customerDb.ExpireAt = null;
                        customerDb.Token = null;
                    }
                    _verifyTechContext.SaveChanges();
                    response.Success = true;
                    response.Message = "Customer has been successfully saved.";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.ToString();
            }
            return response;
        }

        //Get customer with customer id
        public SplashAccountResponse GetCustomer(string customerId)
        {
            var response = new SplashAccountResponse();

            var client = new RestClient();
            client.EndPoint = string.Format("{0}{1}/{2}", ApplicationSettings.SplashApiUrl, "customers", customerId);
            client.Method = HttpVerb.GET;

            var json = client.MakeRequest();

            var customer = JsonConvert.DeserializeObject<GetSplashCustomerResponse>(json);

            if (customer.response.errors.Count != 0)
            {
                response.Success = false;
                response.Message = "Some error occured while getting customer details.";
            }
            else
            {
                response.Success = true;
                response.Message = "Query has been executed successfully.";
            }
            return response;
        }

        // disable customer credit card in DATABASE
        public BaseResponse InactivateCustomerCreditCard(int? customerId)
        {
            var response = new BaseResponse();
            try
            {
                var customer = _verifyTechContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer doesn't exists.";
                    return response;
                }

                customer.IsCcActive = false;
                _verifyTechContext.SaveChanges();

                response.Success = true;
                response.Message = "Payment information has been successfully deleted.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
            }
            return response;
        }

        // activate customer credit card in DATABASE
        public BaseResponse ActivateCustomerCreditCard(int? customerId)
        {
            var response = new BaseResponse();
            try
            {
                var customer = _verifyTechContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer doesn't exists.";
                    return response;
                }
                customer.IsCcActive = true;
                _verifyTechContext.SaveChanges();

                response.Success = true;
                response.Message = "Payment information has been successfully activated.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message.ToString();
                throw;
            }
            return response;
        }

        #endregion
    }
}