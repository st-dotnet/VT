using VT.QuickBooks.DTOs;

namespace VT.QuickBooks.Interfaces
{
    public interface IInvoice
    {
        QuickbookBaseResponse CreateInvoice(string jsonInvoice, string authorizationToken);
    }
}
