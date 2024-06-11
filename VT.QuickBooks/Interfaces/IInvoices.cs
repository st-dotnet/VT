using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.Invoices;

namespace VT.QuickBooks.Interfaces
{
    public interface IInvoices
    {
        QuickbookBaseResponse CreateInvoice(string jsonInvoiceCreation, string authorizationToken);
        VoidInvoiceResponse VoidInvoice(string jsonVoidRequest);
        DeleteInvoiceResponse DeleteInvoice(string jsonDeleteInvoice);
        GetInvoiceResponse GetInvoice(int invoiceId);
    }
}
