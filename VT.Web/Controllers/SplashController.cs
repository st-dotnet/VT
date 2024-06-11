using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Amazon.ElasticTranscoder.Model.Internal.MarshallTransformations;
using AutoMapper;
using VT.Common;
using VT.Common.Utils;
using VT.Data;
using VT.Services.DTOs.SplashPayments;
using VT.Services.Interfaces;
using VT.Web.Models;

namespace VT.Web.Controllers
{
    public class SplashController : BaseController
    {
        #region Field

        private readonly ISplashPaymentService _splashPaymentService;

        #endregion

        #region Constructor

        public SplashController(ISplashPaymentService splashPaymentService)
        {
            _splashPaymentService = splashPaymentService;
        }

        #endregion

        #region Action Method(s)

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [Route("~/Splash/MerchantSetup/{id?}")]
        public ActionResult MerchantSetup(int? id)
        {
            ViewData["EntityTypes"] = GetEntityTypes();
            ViewData["AccountTypes"] = GetAccountTypes();
            ViewData["StateList"] = GetStates();

            ViewBag.DeafultMCC = ApplicationSettings.DefaultMCC;
            var companyId = id ?? CurrentIdentity.CompanyId;

            if (companyId != null)
            {
                var model = GetMerchantAccountModel(companyId.Value);
                return View("MerchantSetup", model);
            }
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// Save merchant
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Splash/SaveMerchant")]
        public ActionResult SaveMerchant(SplashMerchantModel model)
        {
            var ownership = int.Parse(model.MemberOwnerShip);
            var request = new SplashCreateMerchantRequest();

            request.AnnualCCSales = model.AnnualCCSales;
            request.Established = model.Established;
            request.MerchantCategoryCode = model.MerchantCategoryCode;
            request.EntityAddress1 = model.EntityAddress1;
            request.EntityAddress2 = model.EntityAddress2;
            request.EntityCity = model.EntityCity;
            request.EntityCountry = "USA";
            request.DBA = model.DBA;
            request.EntityEmail = model.EntityEmail;
            request.EntityEmployerId = model.EntityEmployerId;
            request.EntityName = model.EntityName;
            request.EntityPhone = model.EntityPhone;
            request.EntityType = model.EntityType;
            request.EntityEIN = model.EntityEIN;
            request.EntityState = model.EntityState;
            request.EntityWebsite = model.EntityWebsite;
            request.EntityZip = model.EntityZip;
            request.CardOrAccountNumber = model.CardOrAccountNumber;
            request.AccountsPaymentMethod = model.AccountsPaymentMethod;
            request.AccountsRoutingCode = model.AccountsRoutingCode;
            request.MemberTitle = model.MemberTitle;
            request.MemberDateOfBirth = model.MemberDateOfBirth;
            request.MemberDriverLicense = model.MemberDriverLicense;
            request.MemberDriverLicenseState = model.MemberDriverLicenseState;
            request.MemberEmail = model.MemberEmail;
            request.MemberFirstName = model.MemberFirstName;
            request.MemberLastName = model.MemberLastName;
            request.MemberOwnerShip = (ownership * 100).ToString();
            request.MemberSocialSecurityNumber = model.MemberSocialSecurityNumber;
            request.CompanyId = CurrentIdentity.CompanyId.HasValue ? CurrentIdentity.CompanyId.Value : model.CompanyId;

            var response = _splashPaymentService.CreateMerchant(request);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }


        // update merchant
        [HttpPost]
        [Route("~/Merchant/UpdateMerchant")]
        public ActionResult UpdateMerchant(SplashMerchantModel model)
        {
            try
            {
                var data = Mapper.Map<UpdateMerchantInfoRequest>(model);
                var response = _splashPaymentService.UpdateMerchantInfo(data);

                return Json(new
                {
                    success = response.Success,
                    message = response.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message.ToString()
                });
            }
        }

        /// <summary>
        /// Save credit card for company or customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Splash/SaveCustomer")]
        public ActionResult SaveCustomer(SplashCustomerModel model)
        {
            var request = new SplashCustomerCreateRequest
            {
                CustomerId = model.CustomerId,
                CustomerFirstName = model.CustomerFirstName,
                CustomerLastName = model.CustomerLastName,
                CustomerAddress1 = model.CustomerAddress1,
                CustomerPhone = model.CustomerPhone,
                CustomerEmail = model.CustomerEmail,
                PaymentMethod = model.PaymentMethod.ToString(),
                PaymentNumber = model.PaymentNumber,
                PaymentCvv = model.PaymentCvv,
                PaymentExpiration = model.Month + "" + model.Year,
                CompanyId = CurrentIdentity.CompanyId.HasValue ? CurrentIdentity.CompanyId.Value : model.CompanyId
            };
            var response = model.IsCustomerMode ? _splashPaymentService.CreateCcCustomerForCustomer(request) :
                _splashPaymentService.CreateCcCustomerForCompany(request);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        /// <summary>
        /// Save customer card detail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Splash/SavePaymentDetail")]
        public ActionResult SavePaymentDetail(SplashCustomerModel model)
        {
            var request = new AddCustomerCreditCardRequest
            {
                CustomerId = model.CustomerId,
                PaymentNumber = model.PaymentNumber,
                PaymentMethod = model.PaymentMethod.ToString(),
                PaymentCvv = model.PaymentCvv,
                PaymentExpiration = model.PaymentExpiration,
                CompanyId = CurrentIdentity.CompanyId.HasValue ? CurrentIdentity.CompanyId.Value : 0
            };
            var response = model.CustomerId > 0 ? _splashPaymentService.AddCustomerCreditCard(request) : _splashPaymentService.AddCompanyCreditCard(request);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        /// <summary>
        /// Save customer detail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/Splash/SaveCustomerDetail")]
        public ActionResult SaveCustomerDetail(SplashCustomerModel model)
        {
            var request = new UpdateSplashCustomerRequest
            {
                CustomerId = model.CustomerId,
                FirstName = model.CustomerFirstName,
                LastName = model.CustomerLastName,
                Email = model.CustomerEmail,
                //  CompanyId = CurrentIdentity.CompanyId.HasValue ? CurrentIdentity.CompanyId.Value : 0 
                CompanyId = model.CompanyId
            };

            var response = model.CustomerId > 0 ? _splashPaymentService.UpdateCustomerCustomer(request) :
                                _splashPaymentService.UpdateCompanyCustomer(request);
            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        /// 
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpGet]
        [Route("~/Splash/SetupMerchantCc/{id?}")]
        public ActionResult SetupMerchantCc(int? id)
        {
            PopulateViews();
            var companyId = id ?? CurrentIdentity.CompanyId;
            if (companyId != null)
            {
                var model = GetCompanyModel(companyId.Value);
                return View("SetupCc", model);
            }
            return RedirectToAction("Login", "Auth");
        }

        #region Company Setup Creditcard Methods

        // set up company credit card
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpGet]
        [Route("~/Splash/SetupCompanyCreditCard/{id?}")]
        public ActionResult SetupCompanyCreditCard(int? id)
        {
            PopulateViews();
            var companyId = id ?? CurrentIdentity.CompanyId;
            if (companyId != null)
            {
                var model = GetCompanyModel(companyId.Value);
                return View("SetupCc", model);
            }
            return RedirectToAction("Login", "Auth");
        }

        // save company customer credit card
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost]
        [Route("~/Splash/SaveCustomerCreditCardForCompany")]
        public ActionResult SaveCustomerCreditCardForCompany(SplashCustomerModel model)
        {
            var request = new SplashCustomerCreateRequest
            {
                CustomerId = model.CustomerId,
                CustomerFirstName = model.CustomerFirstName,
                CustomerLastName = model.CustomerLastName,
                CustomerAddress1 = model.CustomerAddress1,
                CustomerPhone = model.CustomerPhone,
                CustomerEmail = model.CustomerEmail,
                PaymentMethod = model.PaymentMethod.ToString(),
                PaymentNumber = model.PaymentNumber,
                PaymentCvv = model.PaymentCvv,
                PaymentExpiration = model.Month + "" + model.Year,
                CompanyId = CurrentIdentity.CompanyId.HasValue ? CurrentIdentity.CompanyId.Value : model.CompanyId
            };
            var response = _splashPaymentService.CreateCcCustomerForCompany(request);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        // delete/disable company credit card
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost]
        [Route("~/Splash/DisableCompanyCreditCard")]
        public ActionResult DisableCompanyCreditCard(CommonIdModel model)
        {
            var response = _splashPaymentService.DisableCompanyCreditCard(model.CompanyId, model.CustomerId);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        #endregion

        #region Setup Customer Credit Card Method(s)

        //CUSTOMER CREDIT CARD
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpGet]
        [Route("~/ConfigureSplashCustomerCc/{id}")]
        public ActionResult ConfigureSplashCustomerCc(int id)
        {
            PopulateViews();
            var model = GetCustomerModel(id);
            return View("SetupCustomerCreditCard", model);
        }


        // save customer credit card
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost]
        [Route("~/Splash/SaveCustomerCreditCard")]
        public ActionResult SaveCustomerCreditCard(SplashCustomerModel model)
        {
            var request = new SplashCustomerCreateRequest
            {
                CustomerId = model.CustomerId,
                CustomerFirstName = model.CustomerFirstName,
                CustomerLastName = model.CustomerLastName,
                CustomerAddress1 = model.CustomerAddress1,
                CustomerPhone = model.CustomerPhone,
                CustomerEmail = model.CustomerEmail,
                PaymentMethod = model.PaymentMethod.ToString(),
                PaymentNumber = model.PaymentNumber,
                PaymentCvv = model.PaymentCvv,
                PaymentExpiration = model.Month + "" + model.Year,
                CompanyId = CurrentIdentity.CompanyId.HasValue ? CurrentIdentity.CompanyId.Value : model.CompanyId
            };
            var response = _splashPaymentService.CreateCcCustomerForCustomer(request);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        // delete/disable customer credit card
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost]
        [Route("~/Splash/DisableCustomerCreditCard")]
        public ActionResult DisableCustomerCreditCard(CommonIdModel model)
        {
            var response = _splashPaymentService.InactivateCustomerCreditCard(model.CustomerId);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        // activate  customer credit card
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost]
        [Route("~/Splash/ActivateCustomerCreditCard")]
        public ActionResult ActivateCustomerCreditCard(CommonIdModel model)
        {
            //var response = _splashPaymentService.DisableCustomerCreditCard(model.CompanyId, model.CustomerId);
            var response = _splashPaymentService.ActivateCustomerCreditCard(model.CustomerId);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }

        // trash customer credit card
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost]
        [Route("~/Splash/TrashCustomerCreditCard")]
        public ActionResult TrashCustomerCreditCard(CommonIdModel model)
        {
            var response = _splashPaymentService.DisableCustomerCreditCard(model.CompanyId, model.CustomerId);

            return Json(new
            {
                success = response.Success,
                message = response.Message
            });
        }



        #endregion

        #endregion

        #region Private methods

        private void PopulateViews()
        {
            var list = GetPaymentMethod();
            ViewData["PaymentMethod"] = list;
            ViewData["YearsList"] = GetYearsList();
            ViewData["MonthsList"] = GetMonthsList(); 
        }

        // return years list
        private IEnumerable<SelectListItem> GetYearsList()
        {
            var Years = new List<SelectListItem>
            {
               new SelectListItem { Text = " -- Select --", Value = string.Empty }
            };
            for (var i = DateTime.Now.Year; i <= DateTime.Now.Year + 7; i++)
            {
                var list = new SelectListItem();
                list.Text = i.ToString();
                list.Value = i.ToString().Substring(2);
                Years.Add(list);
            }
            return Years;
        }

        // get months
        private IEnumerable<SelectListItem> GetMonthsList()
        {
            var month = new List<SelectListItem>
            {
                  new SelectListItem { Text = " -- Select --", Value = string.Empty }
            };
            for (var i = 1; i <= 12; i++)
            {
                if (i < 10)
                {
                    var list = new SelectListItem();
                    list.Text = 0 + "" + i;
                    list.Value = 0 + "" + i;
                    month.Add(list);
                }
                else
                {
                    var list = new SelectListItem();
                    list.Text = i.ToString();
                    list.Value = i.ToString();
                    month.Add(list);
                }
            }
            return month;
        }

        private IEnumerable<SelectListItem> GetPaymentMethod()
        {
            var methods = new List<SelectListItem>
            {
                new SelectListItem { Text = " -- Select --", Value = string.Empty }
            };
            var enumValues = Enum.GetValues(typeof(PaymentMethod)) as PaymentMethod[];
            if (enumValues == null) return null;
            methods.AddRange(enumValues.Select(enumValue => new SelectListItem
            {
                Value = ((int)enumValue).ToString(CultureInfo.InvariantCulture),
                Text = EnumUtil.GetDescription(enumValue)
            }).ToList());
            return methods;
        }

        private SplashMerchantModel GetMerchantAccountModel(int id)
        {
            var response = _splashPaymentService.GetMerchantDetail(id);

            if (response.Success)
            {
                var model = Mapper.Map<SplashMerchantModel>(response);
                model.RedirectUrl = User.IsInRole(UserRoles.SuperAdmin.ToString())
                    ? Url.Action("Index", "Organizations")
                    : Url.Action("Index", "Users");

                if (!IsValidDate(model.Established)) model.Established = string.Empty;
                if (!IsValidDate(model.MemberDateOfBirth)) model.MemberDateOfBirth = string.Empty;
                
                model.IsEditMode = true;
                return model;
            }
            return new SplashMerchantModel
            {
                CompanyId = id
            };
        }

        private bool IsValidDate(string date)
        {
            bool flag = false;
            DateTime dateValue;
            string[] formats = { "MM/dd/yyyy" };

            DateTime.TryParseExact(date, formats, new CultureInfo("en-US"), DateTimeStyles.None, out dateValue);

            if (dateValue != null && dateValue != DateTime.MinValue)
            {
                flag = true;
            }
            return flag;
        }

        private SplashCustomerModel GetCompanyModel(int id)
        {
            var response = _splashPaymentService.GetCompanyCcDetail(id);
            if (response.Success)
            {
                var model = Mapper.Map<SplashCustomerModel>(response);
                model.RedirectUrl = User.IsInRole(UserRoles.SuperAdmin.ToString())
                    ? Url.Action("Index", "Organizations")
                    : Url.Action("Index", "Users");
                model.IsEditMode = !string.IsNullOrEmpty(response.PaymentNumber);
                model.PaymentMethod = model.PMethod.ToString();
                return model;
            }
            return new SplashCustomerModel { CompanyId = id };
        }

        private SplashCustomerModel GetCustomerModel(int? id)
        {
            var response = _splashPaymentService.GetCustomerCcDetail(id);

            var model = Mapper.Map<SplashCustomerModel>(response);
            model.RedirectUrl = User.IsInRole(UserRoles.SuperAdmin.ToString())
                ? Url.Action("Index", "Organizations")
                : Url.Action("Index", "Users");
            model.IsEditMode = !string.IsNullOrEmpty(response.PaymentNumber);
            model.IsCustomerMode = true;
            model.CustomerId = id.Value;
            model.PaymentMethod = response.PMethod.ToString();

            return model;
        }

        private IEnumerable<SelectListItem> GetEntityTypes()
        {
            var entityTypes = new List<SelectListItem>()
            {
                new SelectListItem { Value = "", Text = "--Select Business Type--" }
            };

            var types = EnumUtility.GetKeyValuePairList<EntityType>();
            entityTypes.AddRange(types.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = (x.Key).ToString()
            }));

            return entityTypes;
        }

        private IEnumerable<SelectListItem> GetAccountTypes()
        {
            var accountTypes = new List<SelectListItem>() { new SelectListItem { Text = "--Select--", Value = "" } };

            var types = EnumUtility.GetKeyValuePairList<AccountType>();
            accountTypes.AddRange(types.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = (x.Key).ToString()
            }));

            return accountTypes;
        }

        private SplashCustomerModel GetSplashCustomerViewModel(int id)
        {
            var model = new SplashCustomerModel();

            var response = _splashPaymentService.GetCompanyCcDetail(id);

            if (response != null)
            {
                model.CustomerFirstName = response.CustomerFirstName;
                model.CustomerLastName = response.CustomerLastName;
                model.CustomerEmail = response.CustomerEmail;
                model.PaymentNumber = response.PaymentNumber;
                model.PaymentExpiration = response.PaymentExpiration;
            }

            model.CompanyId = id;
            model.RedirectUrl = Url.Action("Index", "Customers");
            return model;
        }

        private IEnumerable<SelectListItem> GetStates()
        {
            var states = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text="--Select State--" },
                new SelectListItem { Value = "AL", Text="Alabama" },
                new SelectListItem { Value = "AK", Text="Alaska" },
                new SelectListItem { Value = "AZ", Text="Arizona" },
                new SelectListItem { Value = "AR", Text="Arkansas" },
                new SelectListItem { Value = "CA", Text="California" },
                new SelectListItem { Value = "CO", Text="Colorado" },
                new SelectListItem { Value = "CT", Text="Connecticut" },
                new SelectListItem { Value = "DE", Text="Delaware" },
                new SelectListItem { Value = "DC", Text="District of Columbia" },
                new SelectListItem { Value = "FL", Text="Florida" },
                new SelectListItem { Value = "GA", Text="Georgia" },
                new SelectListItem { Value = "HI", Text="Hawaii" },
                new SelectListItem { Value = "ID", Text="Idaho" },
                new SelectListItem { Value = "IL", Text="Illinois" },
                new SelectListItem { Value = "IN", Text="Indiana" },
                new SelectListItem { Value = "IA", Text="Iowa" },
                new SelectListItem { Value = "KS", Text="Kansas" },
                new SelectListItem { Value = "KY", Text="Kentucky" },
                new SelectListItem { Value = "LA", Text="Louisiana" },
                new SelectListItem { Value = "ME", Text="Maine" },
                new SelectListItem { Value = "MD", Text="Maryland" },
                new SelectListItem { Value = "MA", Text="Massachusetts" },
                new SelectListItem { Value = "MI", Text="Michigan" },
                new SelectListItem { Value = "MN", Text="Minnesota" },
                new SelectListItem { Value = "MS", Text="Mississippi" },
                new SelectListItem { Value = "MO", Text="Missouri" },
                new SelectListItem { Value = "MT", Text="Montana" },
                new SelectListItem { Value = "NE", Text="Nebraska" },
                new SelectListItem { Value = "NV", Text="Nevada" },
                new SelectListItem { Value = "NH", Text="New Hampshire" },
                new SelectListItem { Value = "NJ", Text="New Jersey" },
                new SelectListItem { Value = "NM", Text="New Mexico" },
                new SelectListItem { Value = "NY", Text="New York" },
                new SelectListItem { Value = "NC", Text="North Carolina" },
                new SelectListItem { Value = "ND", Text="North Dakota" },
                new SelectListItem { Value = "OH", Text="Ohio" },
                new SelectListItem { Value = "OK", Text="Oklahoma" },
                new SelectListItem { Value = "OR", Text="Oregon" },
                new SelectListItem { Value = "PA", Text="Pennsylvania" },
                new SelectListItem { Value = "RI", Text="Rhode Island" },
                new SelectListItem { Value = "SC", Text="South Carolina" },
                new SelectListItem { Value = "SD", Text="South Dakota" },
                new SelectListItem { Value = "TN", Text="Tennessee" },
                new SelectListItem { Value = "TX", Text="Texas" },
                new SelectListItem { Value = "UT", Text="Utah" },
                new SelectListItem { Value = "VT", Text="Vermont" },
                new SelectListItem { Value = "VA", Text="Virginia" },
                new SelectListItem { Value = "WA", Text="Washington" },
                new SelectListItem { Value = "WV", Text="West Virginia" },
                new SelectListItem { Value = "WI", Text="Wisconsin" },
                new SelectListItem { Value = "WY", Text="Wyoming" },
                new SelectListItem { Value = "AS", Text="American Samoa" },
                new SelectListItem { Value = "GU", Text="Guam" },
                new SelectListItem { Value = "MP", Text="Northern Mariana Islands" },
                new SelectListItem { Value = "PR", Text="Puerto Rico" },
                new SelectListItem { Value = "VI", Text="U.S. Virgin Islands" }
            };
            return states;
        }

        #endregion  
    }
}