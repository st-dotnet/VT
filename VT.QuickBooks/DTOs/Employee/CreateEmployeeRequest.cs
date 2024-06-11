using System;
using VT.QuickBooks.DTOs.Employee;

namespace VT.QuickBooks.DTOs.Employee
{
    //public class QBPrimaryAddr
    //{
    //    public string Id { get; set; }
    //    public string Line1 { get; set; }
    //    public string City { get; set; }
    //    public string CountrySubDivisionCode { get; set; }
    //    public string PostalCode { get; set; }
    //}

    //public class QBPrimaryPhone
    //{
    //    public string FreeFormNumber { get; set; }
    //}

    //public class QBMetaData
    //{
    //    public DateTime CreateTime { get; set; }
    //    public DateTime LastUpdatedTime { get; set; }
    //}
    public class QBPrimaryEmailAddr
    {
        public string Address { get; set; }
    }

    //public class QBEmployee
    //{
    //    public string SSN { get; set; }
    //    public QBPrimaryAddr PrimaryAddr { get; set; }
    //    public bool BillableTime { get; set; }
    //    public string domain { get; set; }
    //    public bool sparse { get; set; }
    //    public string Id { get; set; }
    //    public string SyncToken { get; set; }
    //    public QBMetaData MetaData { get; set; }
    //    public string GivenName { get; set; }
    //    public string FamilyName { get; set; }
    //    public string DisplayName { get; set; }
    //    public string PrintOnCheckName { get; set; }
    //    public bool Active { get; set; }
    //    public QBPrimaryPhone PrimaryPhone { get; set; }
    //    public QBPrimaryEmailAddr PrimaryEmailAddress { get; set; }
    //}

    //public class CreateEmployeeRequest
    //{
    //    public QBEmployee Employee { get; set; }
    //    public DateTime time { get; set; }
    //}


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
}