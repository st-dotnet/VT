using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace VT.QuickBooks.DTOs.Customers
{
    [XmlRoot(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
    public class MetaData
    {
        [XmlElement(ElementName = "CreateTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CreateTime { get; set; }
        [XmlElement(ElementName = "LastUpdatedTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string LastUpdatedTime { get; set; }
    }

    [XmlRoot(ElementName = "PrimaryPhone", Namespace = "http://schema.intuit.com/finance/v3")]
    public class PrimaryPhone
    {
        [XmlElement(ElementName = "FreeFormNumber", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FreeFormNumber { get; set; }
    }


    [XmlRoot(ElementName = "PrimaryEmailAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class PrimaryEmailAddr
    {
        [XmlElement(ElementName = "Address", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Address { get; set; }
    }

    [XmlRoot(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class BillAddr
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "Line1", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Line1 { get; set; }
        [XmlElement(ElementName = "City", Namespace = "http://schema.intuit.com/finance/v3")]
        public string City { get; set; }
        [XmlElement(ElementName = "Country", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Country { get; set; }
        [XmlElement(ElementName = "CountrySubDivisionCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CountrySubDivisionCode { get; set; }
        [XmlElement(ElementName = "PostalCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PostalCode { get; set; }
    }

    [XmlRoot(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class CurrencyRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Customer", Namespace = "http://schema.intuit.com/finance/v3")]
    public class Customer
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "SyncToken", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SyncToken { get; set; }
        [XmlElement(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
        public MetaData MetaData { get; set; }
        [XmlElement(ElementName = "Title", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Title { get; set; }
        [XmlElement(ElementName = "GivenName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string GivenName { get; set; }
        [XmlElement(ElementName = "FamilyName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FamilyName { get; set; }
        [XmlElement(ElementName = "Suffix", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Suffix { get; set; }
        [XmlElement(ElementName = "FullyQualifiedName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FullyQualifiedName { get; set; }
        [XmlElement(ElementName = "CompanyName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CompanyName { get; set; }
        [XmlElement(ElementName = "DisplayName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DisplayName { get; set; }
        [XmlElement(ElementName = "PrintOnCheckName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PrintOnCheckName { get; set; }
        [XmlElement(ElementName = "Active", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Active { get; set; }
        [XmlElement(ElementName = "PrimaryPhone", Namespace = "http://schema.intuit.com/finance/v3")]
        public PrimaryPhone PrimaryPhone { get; set; }
        [XmlElement(ElementName = "PrimaryEmailAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public PrimaryEmailAddr PrimaryEmailAddr { get; set; }
        [XmlElement(ElementName = "DefaultTaxCodeRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DefaultTaxCodeRef { get; set; }
        [XmlElement(ElementName = "Taxable", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Taxable { get; set; }
        [XmlElement(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public BillAddr BillAddr { get; set; }
        [XmlElement(ElementName = "Notes", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Notes { get; set; }
        [XmlElement(ElementName = "Job", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Job { get; set; }
        [XmlElement(ElementName = "BillWithParent", Namespace = "http://schema.intuit.com/finance/v3")]
        public string BillWithParent { get; set; }
        [XmlElement(ElementName = "Balance", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Balance { get; set; }
        [XmlElement(ElementName = "BalanceWithJobs", Namespace = "http://schema.intuit.com/finance/v3")]
        public string BalanceWithJobs { get; set; }
        [XmlElement(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public CurrencyRef CurrencyRef { get; set; }
        [XmlElement(ElementName = "PreferredDeliveryMethod", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PreferredDeliveryMethod { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
        [XmlAttribute(AttributeName = "sparse")]
        public string Sparse { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class GetCustomerResponse : QuickbookBaseResponse
    {
        [XmlElement(ElementName = "Customer", Namespace = "http://schema.intuit.com/finance/v3")]
        public Customer Customer { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }
}
