using System.Collections.Generic;
using System.Web.Mvc;

namespace VT.Web.Models
{
    public class UserCustomerAccessModel
    {
        public int CompanyWorkerUserId { get; set; }
        public List<int> From { get; set; }
        public List<int> To { get; set; }
    }

    public class UserCustomerAccessDetailModel : UserCustomerAccessModel
    {
        public List<SelectListCustomModel> FromList { get; set; }
        public List<SelectListCustomModel> ToList { get; set; } 
    }


    public class ViewCustomerAccessModel
    {
        public int CompanyWorkerUserId { get; set; }
        public List<SelectListCustomModel> FromList { get; set; }
        public List<SelectListCustomModel> ToList { get; set; }
    }
     
    public class CustomerAccessModel
    {
        public int CompanyWorkerUserId { get; set; } 
        public List<int> FromList { get; set; }
        public List<int> ToList { get; set; }
    }
}
