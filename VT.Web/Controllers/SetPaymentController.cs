using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using VT.Common;
using VT.Common.Utils;
using VT.Data;
using VT.Services.DTOs;
using VT.Services.DTOs.SplashPayments;
using VT.Services.Interfaces;
using VT.Web.Models;

namespace VT.Web.Controllers
{
    [AllowAnonymous]
    public class SetPaymentController : Controller
    {
        #region Fields

        private readonly IPaymentGatewayService _paymentService;
        private readonly ICustomerService _customerService;
        private readonly ISplashPaymentService _splashPaymentService;

        #endregion

        #region Constructor

        public SetPaymentController(IPaymentGatewayService paymentService,
            ISplashPaymentService splashPaymentService,
                         ICustomerService customerService)
        {
            _paymentService = paymentService;
            _splashPaymentService = splashPaymentService;
            _customerService = customerService;
        }

        #endregion

        #region Public Methods

        // GET: SetPayment
        [HttpGet]
        [Route("~/SetPaymentMethod/{id}")]
        public ActionResult Index(string id)
        {
            var model = GetGatewayCustomerViewModel(id);
            return View(model);
        }

        [HttpPost]
        [Route("~/SetPaymentMethod")]
        public ActionResult SetPaymentMethod(GatewayCustomerViewModel model)
        {
            if (model.GatewayType == PaymentGatewayType.Braintree)
            {
                //prepare dto
                var request = Mapper.Map<GatewayCustomerRequest>(model);

                //assign nonce
                request.NonceFromTheClient = model.Nonce;

                //get response from braintree service
                var response = _paymentService.SaveCustomerCc(request);

                //prepare message view model
                return Json(new
                {
                    success = response.Success,
                    message = response.Success ? "Customer Account has been successfully added. Reference #: " + response.ReferenceNumber : response.Message
                });
            }
            else
            {
                var response = _splashPaymentService.CreateCcCustomerForCustomer(new SplashCustomerCreateRequest
                {
                    CustomerFirstName = model.FirstName,
                    CustomerLastName = model.LastName,
                    CustomerEmail = model.Email,
                    CompanyId = model.CompanyId.GetValueOrDefault(),
                    CustomerId = model.CustomerId,
                    PaymentCvv = model.CVV,
                    PaymentExpiration = string.Format("{0}{1}",
                        model.Month.ToString().PadLeft(2, '0'),
                        model.Year.ToString().PadLeft(2, '0')),
                    PaymentMethod = model.CardType,
                    PaymentNumber = model.CreditCard
                });

                //prepare message view model
                return Json(new
                {
                    success = response.Success,
                    message = response.Message,
                    internalError = response.InternalError
                });
            }
        }
        [Route("~/setup-success")]
        public ActionResult Success()
        {
            return View();
        }
        [Route("~/setup-error")]
        public ActionResult Error()
        {
            return View();
        }

        private GatewayCustomerViewModel GetGatewayCustomerViewModel(string id)
        {
            var model = new GatewayCustomerViewModel();

            var customer = _customerService.GetCustomer(id);

            if (customer == null)
            {
                model.IsCustomerNotFound = true;
                model.IsCustomerTokenExpired = true;
                return model;
            }

            if (customer.ExpireAt == null || customer.ExpireAt < DateTime.UtcNow)
            {
                model.IsCustomerTokenExpired = true;
                return model;
            }

            if (customer.Company.PaymentGatewayType == Data.PaymentGatewayType.Braintree)
            {
                var response = _paymentService.GetCustomerCreditCardDetail(new CustomerCreditCardDetailRequest
                {
                    CustomerId = customer.CustomerId
                });

                if (response != null)
                {
                    model.FirstName = response.FirstName;
                    model.LastName = response.LastName;
                    model.Email = response.Email;
                    model.CreditCard = response.CreditCardNumber;
                    model.Expiration = response.Expiration;
                    model.CcToken = response.CcToken;
                }
                model.GatewayType = Data.PaymentGatewayType.Braintree;
                model.ClientToken = GatewayConstant.Gateway.ClientToken.generate();
                model.RedirectUrl = string.Empty;
            }
            else
            {
                var response = _splashPaymentService.GetCustomerCcDetail(customer.CustomerId);
                if (response != null)
                {
                    model.FirstName = response.CustomerFirstName;
                    model.LastName = response.CustomerLastName;
                    model.CVV = response.PaymentCvv;
                    model.Email = response.CustomerEmail;
                    model.CreditCard = response.PaymentNumber;
                    model.Month = response.PaymentExpiration != null ? response.PaymentExpiration.Substring(0, 2)  : null;
                    model.Year = response.PaymentExpiration != null ? response.PaymentExpiration.Substring(2, 2) : null;
                    model.PaymentMethod = response.PMethod;

                    model.Expiration = response.PaymentExpiration;
                    model.CardType = ((int)response.PaymentMethod).ToString();
                    model.IsEditMode = !string.IsNullOrEmpty(response.PaymentNumber);
                }
                model.GatewayType = PaymentGatewayType.Splash;

                PopulateViews();
            }

            model.CustomerId = customer.CustomerId;
            model.AccountType = GatewayAccount.Customer;
            model.CompanyId = customer.CompanyId;

            return model;
        }

        private void PopulateViews()
        {
            var list = GetPaymentMethod();
            ViewData["PaymentMethod"] = list;
            ViewData["YearsList"] = GetYearsList();
            ViewData["MonthsList"] = GetMonthsList();
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
                    list.Text = "0" + i.ToString();
                    list.Value = "0" + i.ToString();
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

        #endregion
    }
}