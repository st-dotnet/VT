using System.Collections.Generic;

namespace VT.Web.Models.QBCustomers
{
    public class LinkCustomerModel
    {
        public List<int> SystemCustomers { get; set; }
        public List<int> QbCustomers { get; set; }
    }
    public class IdsModel
    {
        public string CustomerId { get; set; }
        public string QBCustomerId { get; set; }
    }
    public class LinkEmployeeModel
    {
        public List<int> SystemEmployees { get; set; }
        public List<int> QbEmployees { get; set; }
    }
}