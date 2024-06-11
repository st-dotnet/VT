using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using VT.QuickBooks.DTOs.Customers;

namespace VT.QuickBooks.DTOs.Employee
{
    [XmlRoot(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
    public class MetaData
    {
        [XmlElement(ElementName = "CreateTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CreateTime { get; set; }
        [XmlElement(ElementName = "LastUpdatedTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string LastUpdatedTime { get; set; }
    }

    [XmlRoot(ElementName = "PrimaryAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class PrimaryAddrResponse
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "Line1", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Line1 { get; set; }
        [XmlElement(ElementName = "City", Namespace = "http://schema.intuit.com/finance/v3")]
        public string City { get; set; }
        [XmlElement(ElementName = "CountrySubDivisionCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CountrySubDivisionCode { get; set; }
        [XmlElement(ElementName = "PostalCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PostalCode { get; set; }
    }

    [XmlRoot(ElementName = "Employee", Namespace = "http://schema.intuit.com/finance/v3")]
    public class Employee
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "SyncToken", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SyncToken { get; set; }
        [XmlElement(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
        public MetaData MetaData { get; set; }
        [XmlElement(ElementName = "GivenName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string GivenName { get; set; }
        [XmlElement(ElementName = "FamilyName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FamilyName { get; set; }
        [XmlElement(ElementName = "DisplayName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DisplayName { get; set; }
        [XmlElement(ElementName = "PrintOnCheckName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PrintOnCheckName { get; set; }
        [XmlElement(ElementName = "Active", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Active { get; set; }
        [XmlElement(ElementName = "PrimaryPhone", Namespace = "http://schema.intuit.com/finance/v3")]
        public PrimaryPhone PrimaryPhone { get; set; }
        [XmlElement(ElementName = "SSN", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SSN { get; set; }
        [XmlElement(ElementName = "PrimaryAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public PrimaryAddrResponse PrimaryAddr { get; set; }
        [XmlElement(ElementName = "PrimaryEmailAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public PrimaryEmailAddr PrimaryEmailAddr { get; set; }
        [XmlElement(ElementName = "BillableTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string BillableTime { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
        [XmlAttribute(AttributeName = "sparse")]
        public string Sparse { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class CreateEmployeeResponse
    {
        [XmlElement(ElementName = "Employee", Namespace = "http://schema.intuit.com/finance/v3")]
        public Employee Employee { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}
