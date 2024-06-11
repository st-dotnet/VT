using System.Collections.Generic;
using System.Web.Mvc;

namespace VT.Web.Models
{
    public class SaveUserModel
    {
        public SaveUserModel()
        {
            FromList = new List<SelectListCustomModel>();
            ToList = new List<SelectListCustomModel>();
        }

        public int CompanyWorkerId { get; set; }
        public string Username { get; set; } //Email
        public string AuthKey { get; set; } //Password
        public string Confirm { get; set; } //Password
        public int? CompanyId { get; set; }
        public int OrgId { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public int? IsUserAddedByAdministrator { get; set; }


        public List<int> From { get; set; }
        public List<int> To { get; set; }
        public List<SelectListCustomModel> FromList { get; set; }
        public List<SelectListCustomModel> ToList { get; set; }
    }
}