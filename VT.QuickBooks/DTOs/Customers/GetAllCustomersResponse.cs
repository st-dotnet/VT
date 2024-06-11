using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace VT.QuickBooks.DTOs.Customers
{
    [XmlRoot(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
    public class MetaDataResponse
    {
        [XmlElement(ElementName = "CreateTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CreateTime { get; set; }
        [XmlElement(ElementName = "LastUpdatedTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string LastUpdatedTime { get; set; }
    }

    [XmlRoot(ElementName = "PrimaryPhone", Namespace = "http://schema.intuit.com/finance/v3")]
    public class PrimaryPhoneResponse
    {
        [XmlElement(ElementName = "FreeFormNumber", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FreeFormNumber { get; set; }
    }

    [XmlRoot(ElementName = "PrimaryEmailAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class PrimaryEmailAddrResponse
    {
        [XmlElement(ElementName = "Address", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Address { get; set; }
    }

    [XmlRoot(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class BillAddrResponse
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
        [XmlElement(ElementName = "Lat", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Lat { get; set; }
        [XmlElement(ElementName = "Long", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Long { get; set; }
        [XmlElement(ElementName = "Country", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Country { get; set; }
    }

    [XmlRoot(ElementName = "ShipAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class ShipAddrResponse
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
        [XmlElement(ElementName = "Lat", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Lat { get; set; }
        [XmlElement(ElementName = "Long", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Long { get; set; }
        [XmlElement(ElementName = "Country", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Country { get; set; }
    }

    [XmlRoot(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class CurrencyRefResponse
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Customer", Namespace = "http://schema.intuit.com/finance/v3")]
    public class CustomerResponse
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "SyncToken", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SyncToken { get; set; }
        [XmlElement(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
        public MetaDataResponse MetaData { get; set; }
        [XmlElement(ElementName = "GivenName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string GivenName { get; set; }
        [XmlElement(ElementName = "FamilyName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FamilyName { get; set; }
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
        public PrimaryPhoneResponse PrimaryPhone { get; set; }
        [XmlElement(ElementName = "PrimaryEmailAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public PrimaryEmailAddrResponse PrimaryEmailAddr { get; set; }
        [XmlElement(ElementName = "Taxable", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Taxable { get; set; }
        [XmlElement(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public BillAddr BillAddr { get; set; }
        [XmlElement(ElementName = "ShipAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public ShipAddrResponse ShipAddr { get; set; }
        [XmlElement(ElementName = "Job", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Job { get; set; }
        [XmlElement(ElementName = "BillWithParent", Namespace = "http://schema.intuit.com/finance/v3")]
        public string BillWithParent { get; set; }
        [XmlElement(ElementName = "Balance", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Balance { get; set; }
        [XmlElement(ElementName = "BalanceWithJobs", Namespace = "http://schema.intuit.com/finance/v3")]
        public string BalanceWithJobs { get; set; }
        [XmlElement(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public CurrencyRefResponse CurrencyRef { get; set; }
        [XmlElement(ElementName = "PreferredDeliveryMethod", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PreferredDeliveryMethod { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
        [XmlAttribute(AttributeName = "sparse")]
        public string Sparse { get; set; }
        [XmlElement(ElementName = "Mobile", Namespace = "http://schema.intuit.com/finance/v3")]
        public Mobile Mobile { get; set; }
        [XmlElement(ElementName = "Fax", Namespace = "http://schema.intuit.com/finance/v3")]
        public Fax Fax { get; set; }
        [XmlElement(ElementName = "WebAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public WebAddr WebAddr { get; set; }
        [XmlElement(ElementName = "ParentRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public string ParentRef { get; set; }
        [XmlElement(ElementName = "Level", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Level { get; set; }
        [XmlElement(ElementName = "Title", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Title { get; set; }
        [XmlElement(ElementName = "MiddleName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string MiddleName { get; set; }
        [XmlElement(ElementName = "Suffix", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Suffix { get; set; }
        [XmlElement(ElementName = "Notes", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Notes { get; set; }
        [XmlElement(ElementName = "DefaultTaxCodeRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DefaultTaxCodeRef { get; set; }
    }

    [XmlRoot(ElementName = "Mobile", Namespace = "http://schema.intuit.com/finance/v3")]
    public class Mobile
    {
        [XmlElement(ElementName = "FreeFormNumber", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FreeFormNumber { get; set; }
    }

    [XmlRoot(ElementName = "Fax", Namespace = "http://schema.intuit.com/finance/v3")]
    public class Fax
    {
        [XmlElement(ElementName = "FreeFormNumber", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FreeFormNumber { get; set; }
    }

    [XmlRoot(ElementName = "WebAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class WebAddr
    {
        [XmlElement(ElementName = "URI", Namespace = "http://schema.intuit.com/finance/v3")]
        public string URI { get; set; }
    }

    [XmlRoot(ElementName = "QueryResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class QueryResponse
    {
        [XmlElement(ElementName = "Customer", Namespace = "http://schema.intuit.com/finance/v3")]
        public List<CustomerResponse> Customer { get; set; }
        [XmlAttribute(AttributeName = "startPosition")]
        public string StartPosition { get; set; }
        [XmlAttribute(AttributeName = "maxResults")]
        public string MaxResults { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class AllCustomerResponse : QuickbookBaseResponse
    {
        [XmlElement(ElementName = "QueryResponse", Namespace = "http://schema.intuit.com/finance/v3")]
        public QueryResponse QueryResponse { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }
}