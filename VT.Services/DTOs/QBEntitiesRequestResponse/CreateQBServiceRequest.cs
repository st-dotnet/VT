using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class IncomeAccountRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }
    public class ExpenseAccountRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }
    public class CreateQBServiceRequest
    {
        public string Name { get; set; }
        public IncomeAccountRef IncomeAccountRef { get; set; }
        public ExpenseAccountRef ExpenseAccountRef { get; set; }
         public string Type { get; set; }
        public string Description { get; set; }
        //public bool TrackQtyOnHand { get; set; }
        //public int QtyOnHand { get; set; }
        //public string InvStartDate { get; set; }
    }
}
