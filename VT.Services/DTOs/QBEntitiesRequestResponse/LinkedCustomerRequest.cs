using System.Collections.Generic;

namespace VT.Services.DTOs.QBEntitiesRequestResponse
{
    public class LinkedCustomerResponse
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public string ContactFirstName { get; set; }
        public string ContactMiddleName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactMobile { get; set; }

        public string QbCustomerId { get; set; }
        public bool IsMatch { get; set; }
        public bool IsSystemCustomer { get; set; }

        public bool IsActive { get; set; }
    }

    public class SystemCustomerModel
    {
        public SystemCustomerModel()
        {
            IsLinked = true;
        }

        public int CompanyId { get; set; }
        public string QbCustomerId { get; set; }
        public int?   SCCustomerId { get; set; }
        public string SCName { get; set; }
        public string SCAddress { get; set; }
        public string SCity { get; set; }
        public string SState { get; set; }
        public string SCPostalCode { get; set; }
        public string SCountry { get; set; }
        public string SCPhone { get; set; }
        public string SCEmail { get; set; }
        public bool IsActive { get; set; }
        public bool IsMatch { get; set; }
        public bool IsLinked { get; set; }
    }

    public class SystemCustomerModel1
    {
        public SystemCustomerModel1()
        {
            IsLinked1 = true;
        }

        public int CompanyId1 { get; set; }
        public string QbCustomerId1 { get; set; }
        public int? SCCustomerId1 { get; set; }
        public string SCName1 { get; set; }
        public string SCAddress1 { get; set; }
        public string SCity1 { get; set; }
        public string SState1 { get; set; }
        public string SCPostalCode1 { get; set; }
        public string SCountry1 { get; set; }
        public string SCPhone1 { get; set; }
        public string SCEmail1 { get; set; }
        public bool IsActive1 { get; set; }
        public bool IsMatch1 { get; set; }
        public bool IsLinked1 { get; set; }
    }

    public class QBCustomerModel : SystemCustomerModel
    {

    }

    public class UnlinkedCustomer
    {
        public UnlinkedCustomer()
        {
            UnlinkedSystemCustomers = new List<SystemCustomerModel>();
            unlinkedQBCustomers = new  List<QBCustomerModel>();
        }
        public List<SystemCustomerModel> UnlinkedSystemCustomers { get; set; }
        public List<QBCustomerModel> unlinkedQBCustomers { get; set; }
    }

    public class LinkedCustomer
    {
        public LinkedCustomer()
        {
            LinkedSystemCustomer = new SystemCustomerModel();
            LinkedQBCustomer = new QBCustomerModel();
        }
        public SystemCustomerModel LinkedSystemCustomer { get; set; }
        public QBCustomerModel LinkedQBCustomer { get; set; }
    }

    public class CustomerSynchronizationList : BaseResponse
    {
        public CustomerSynchronizationList()
        {
            LinkedCustomers = new List<LinkedCustomer>();
            UnlinkedSystemCustomers = new List<SystemCustomerModel>();
            UnlinkedQBCustomers = new List<QBCustomerModel>();
        }

        public List<LinkedCustomer> LinkedCustomers { get; set; }

        public List<SystemCustomerModel> UnlinkedSystemCustomers { get; set; }

        public List<QBCustomerModel> UnlinkedQBCustomers { get; set; }
    }
}