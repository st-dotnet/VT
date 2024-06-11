using System.Collections.Generic;

namespace VT.Services.DTOs
{
    public class QBPrimaryEmailAddr
    {
        public string Address { get; set; }
    }

    public class PrimaryAddr
    {
        public string Id { get; set; }
        public string Line1 { get; set; }
        public string City { get; set; }
        public string CountrySubDivisionCode { get; set; }
        public string PostalCode { get; set; }
    }
    public class PrimaryEmailAddr
    {
        public string Address { get; set; }
    }

    public class PrimaryPhone
    {
        public string FreeFormNumber { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string SSN { get; set; }
        public PrimaryAddr PrimaryAddr { get; set; }
        public string GivenName { get; set; }
        public string Id { get; set; }
        public string FamilyName { get; set; }
        public string PrintOnCheckName { get; set; }
        public string DisplayName { get; set; }
        public PrimaryPhone PrimaryPhone { get; set; }
        public PrimaryEmailAddr PrimaryEmailAddr { get; set; }
    }

    public class SystemEmployeeModel
    {
        public string SSN { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string GivenName { get; set; }
        public int CompanyId { get; set; }
        public string Id { get; set; }
        public string FamilyName { get; set; }
        public string PrintOnCheckName { get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int? EmployeeId { get; set; }
        public string QBEmployeeId { get; set; }

        public bool IsActive { get; set; }
        public bool IsMatch { get; set; }
        public bool IsLinked { get; set; }
    }
    public class SystemEmployeeModel1
    {
        public string SSN1 { get; set; }
        public string Address1 { get; set; }
        public string City1 { get; set; }
        public string State1{ get; set; }
        public string PostalCode1 { get; set; }
        public string GivenName1 { get; set; }
        public int CompanyId1 { get; set; }
        public string Id1 { get; set; }
        public string FamilyName1 { get; set; }
        public string PrintOnCheckName1 { get; set; }
        public string DisplayName1 { get; set; }
        public string PhoneNumber1 { get; set; }
        public string Email1 { get; set; }
        public int? EmployeeId1 { get; set; }
        public string QBEmployeeId1 { get; set; }

        public bool IsActive1 { get; set; }
        public bool IsMatch1 { get; set; }
        public bool IsLinked1 { get; set; }
    }

    public class QBEmployeeModel : SystemEmployeeModel
    {
    }

    public class UnlinkedEmployee
    {
        public UnlinkedEmployee()
        {
            UnlinkedSystemEmployees = new List<SystemEmployeeModel>();
            unlinkedQBEmployees = new List<QBEmployeeModel>();
        }
        public List<SystemEmployeeModel> UnlinkedSystemEmployees { get; set; }
        public List<QBEmployeeModel> unlinkedQBEmployees { get; set; }
    }

    public class LinkedEmployee
    {
        public LinkedEmployee()
        {
            LinkedSystemEmployee = new SystemEmployeeModel();
            LinkedQBEmployee = new QBEmployeeModel();
        }
        public SystemEmployeeModel LinkedSystemEmployee { get; set; }
        public QBEmployeeModel LinkedQBEmployee { get; set; }
    }

    public class EmployeeSynchronizationList : BaseResponse
    {
        public EmployeeSynchronizationList()
        {
            LinkedEmployees = new List<LinkedEmployee>();
            UnlinkedSystemEmployees = new List<SystemEmployeeModel>();
            UnlinkedQBEmployees = new List<QBEmployeeModel>();
        }
        public List<LinkedEmployee> LinkedEmployees { get; set; }

        public List<SystemEmployeeModel> UnlinkedSystemEmployees { get; set; }

        public List<QBEmployeeModel> UnlinkedQBEmployees { get; set; }
    }
}