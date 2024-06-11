namespace VT.Web.Models.QBCustomers
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
    public class CreateServiceModel
    {
        public string Name { get; set; }
        public IncomeAccountRef IncomeAccountRef { get; set; }
        public ExpenseAccountRef ExpenseAccountRef { get; set; }
        public string Type { get; set; }
        //public bool TrackQtyOnHand { get; set; }
        //public int QtyOnHand { get; set; }
        //public string InvStartDate { get; set; }
    }
}