using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using VT.Common.Utils;
using VT.Data;
using VT.Services.DTOs;
using VT.Services.Interfaces;
using VT.Web.Models;
using System.Web;

namespace VT.Web.Controllers
{
    public class OrganizationsController : BaseController
    {
        #region Fields

        private readonly ICompanyService _companyService;

        #endregion

        #region Constructor

        public OrganizationsController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        #endregion

        #region Action Methods

        [Route("~/")]
        public ActionResult Index()
        {
            PopulateViews();
            return View();
        }

        [HttpPost]
        [Route("~/Organizations/SaveOrganization")]
        public ActionResult SaveOrganization(SaveOrganizationViewModel model)
        {
            var request = Mapper.Map<CompanySaveRequest>(model);
            var response = _companyService.Save(request);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
                orgId = response.Company != null ? response.Company.CompanyId : 0
            });
        }

        [Authorize(Roles = "CompanyAdmin")]
        [HttpGet]
        [Route("~/UpdateLogo")]
        public ActionResult UpdateLogo()
        {
            if (CurrentIdentity.CompanyId == null) return RedirectToAction("Login", "Auth");

            var id = CurrentIdentity.CompanyId;
            ViewBag.Id = id;
            ViewBag.ImageUrl = CurrentIdentity.ImageUrl;
            return View();
        }

        [HttpPost]
        [Route("~/Organizations/SaveImageName")]
        public ActionResult SaveImageName(ImageDetails model)
        {
            if (model.File == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Please select file to upload."
                });
            }
            var request = Mapper.Map<ImageDetailsRequest>(model);
            var response = _companyService.SaveImageName(request);
            Session["ImageUrl"] = model.File;

            return Json(new
            {
                success = response.Success,
                message = response.Message,
                companyid = model.CompanyId,
                filename = model.File
            });
        }

        [HttpPost]
        public ActionResult UploadCompanyLogo(FormCollection fc, HttpPostedFileBase file)
        {
            return View();
        }

        [HttpPost, Route("~/Organizations/CheckOrgName")]
        public ActionResult CheckOrgName(string name)
        {
            var result = _companyService.IsOrgNameExist(name);
            return Content(result.Success ? "true" : "false");
        }

        [HttpPost]
        [Route("~/Organizations/OrganizationList")]
        public ActionResult OrganizationList([DataSourceRequest] DataSourceRequest request, string additionalInfo)
        {
            bool? filter = null;
            if (additionalInfo == "Active") filter = true;
            if (additionalInfo == "Not Active") filter = false;
            var data = new List<OrganizationListViewModel>();

            data = filter != null ? GetOrganizationList().Where(x => x.IsActive.Value == filter).ToList() : GetOrganizationList().ToList();

            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("~/Organizations/GetOrganizationData/{id?}")]
        public JsonResult GetOrganizationData(int? id)
        {
            var model = id != null ? GetOrganization(id.Value) : new SaveOrganizationViewModel();
            return Json(model);
        }

        [HttpPost]
        [Route("~/Organizations/GetOrganizationDetail/{id}")]
        public ActionResult GetOrganizationDetail(int id)
        {
            var result = _companyService.GetOrganizationDetail(id);
            return PartialView("OrganizationDetail", result);
        }

        [HttpPost]
        [Route("~/Organizations/DeleteOrgs")]
        public ActionResult DeleteOrgs(DeleteOrgViewModel model)
        {
            var response = _companyService.DeleteOrgs(model.Ids);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        // activate org
        [HttpPost]
        [Route("~/Organizations/ActivateOrg/{organizationId}")]
        public ActionResult ActivateOrg(int organizationId)
        {
            var response = _companyService.ActivateOrganization(organizationId);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        // Deactivate org
        [HttpPost]
        [Route("~/Organizations/DeactivateOrg/{organizationId}")]
        public ActionResult DeactivateOrg(int organizationId)
        {
            var response = _companyService.DeactivateOrganization(organizationId);
            return Json(new
            {
                success = response.Success,
                message = response.Message,
            });
        }

        [HttpPost]
        [Route("~/Organizations/GetOrganizations")]
        public ActionResult GetOrganizations()
        {
            var data = _companyService.GetOranizationList();

            var list = new List<SelectListItem>();
            list.AddRange(data.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.CompanyId.ToString(CultureInfo.InvariantCulture)
            }));

            return Json(list);
        }

        #endregion

        #region Private Methods

        private void PopulateViews()
        {
            var list = GetPaymentGatewayType();
            ViewData["PaymentGateway"] = list;
            var list1 = GetOrganizations1();
            ViewData["Organizations"] = list1;
            ViewData["StateList"] = GetStates();
            ViewData["From"] = new List<SelectListItem>();
            ViewData["To"] = new List<SelectListItem>();
        }

        private List<SelectListItem> GetOrganizations1()
        {
            var data = _companyService.GetOranizationList();

            var list = new List<SelectListItem>();
            list.AddRange(data.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.CompanyId.ToString(CultureInfo.InvariantCulture)
            }));

            return list;
        }

        private IEnumerable<OrganizationListViewModel> GetOrganizationList()
        {
            var companies = _companyService.GetAllCompanies();
            var list = companies.Select(x => new OrganizationListViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Customers = x.Customers,
                Services = x.Services,
                Users = x.Users,
                GatewayCustomerId = x.GatewayCustomerId,
                MerchantAccountId = x.MerchantAccountId,
                PaymentGatewayType = x.PaymentGatewayType,
                IsActive = x.IsActive
            }).ToList();
            return list;
        }

        private SaveOrganizationViewModel GetOrganization(int id)
        {
            var data = _companyService.GetOrganizationDetail(id);
            if (data == null) throw new Exception("This organization does not exist");

            var model = Mapper.Map<SaveOrganizationViewModel>(data);
            return model;
        }

        private List<SelectListItem> GetPaymentGatewayType()
        {
            var enumValues = Enum.GetValues(typeof(PaymentGatewayType)) as PaymentGatewayType[];
            if (enumValues == null) return null;
            return enumValues.Select(enumValue => new SelectListItem
            {
                Value = ((int)enumValue).ToString(CultureInfo.InvariantCulture),
                Text = EnumUtil.GetDescription(enumValue)
            }).OrderByDescending(x => x.Value == "2").ToList();
        }

        private IEnumerable<SelectListItem> GetStates()
        {
            var states = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text="--Select--" },
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