using VT.QuickBooks.DTOs;
using VT.QuickBooks.DTOs.Customers;

namespace VT.QuickBooks.Interfaces
{
    public interface ICustomer
    {
        GetCustomerResponse GetCustomer(string customerId, string authorizationToken);
        QuickbookBaseResponse CreateCustomer(string jsonCustomer, string authorizationTokenHeader);
        QuickbookBaseResponse DeleteCustomer(int customerId);
        AllCustomerResponse GetAllCustomers(string authorizationToken);
        QuickbookBaseResponse UpdateCustomer(string jsonCustomer, string authorizationToken);
    }
}