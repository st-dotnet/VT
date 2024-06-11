using System.Collections.Generic;

namespace VT.QuickBooks.DTOs.Invoices
{
    public class CreateInvoiceRequest
    {
        public List<Line> Line { get; set; }
        public CustomerRef CustomerRef { get; set; }
    }

    public class ItemRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    public class SalesItemLineDetail
    {
        public ItemRef ItemRef { get; set; }
    }

    public class Line
    {
        public double Amount { get; set; }
        public string DetailType { get; set; }
        public SalesItemLineDetail SalesItemLineDetail { get; set; }
    }

    public class CustomerRef
    {
        public string value { get; set; }
    }
    public class Invoice : CreateInvoiceRequest
    {
    }
}
