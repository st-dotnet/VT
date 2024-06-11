using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using VT.QuickBooks.DTOs;

namespace Xml2CSharp
{
    [XmlRoot(ElementName = "CustomField", Namespace = "http://schema.intuit.com/finance/v3")]
    public class CustomField
    {
        [XmlElement(ElementName = "DefinitionId", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DefinitionId { get; set; }
        [XmlElement(ElementName = "Name", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Type", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class CurrencyRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ItemRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class ItemRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "SalesItemLineDetail", Namespace = "http://schema.intuit.com/finance/v3")]
    public class SalesItemLineDetail
    {
        [XmlElement(ElementName = "ItemRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public ItemRef ItemRef { get; set; }
        [XmlElement(ElementName = "TaxCodeRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TaxCodeRef { get; set; }
    }

    [XmlRoot(ElementName = "Line", Namespace = "http://schema.intuit.com/finance/v3")]
    public class Line
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "LineNum", Namespace = "http://schema.intuit.com/finance/v3")]
        public string LineNum { get; set; }
        [XmlElement(ElementName = "Description", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Description { get; set; }
        [XmlElement(ElementName = "Amount", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Amount { get; set; }
        [XmlElement(ElementName = "DetailType", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DetailType { get; set; }
        [XmlElement(ElementName = "SalesItemLineDetail", Namespace = "http://schema.intuit.com/finance/v3")]
        public SalesItemLineDetail SalesItemLineDetail { get; set; }
        [XmlElement(ElementName = "SubTotalLineDetail", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SubTotalLineDetail { get; set; }
    }

    [XmlRoot(ElementName = "TxnTaxDetail", Namespace = "http://schema.intuit.com/finance/v3")]
    public class TxnTaxDetail
    {
        [XmlElement(ElementName = "TotalTax", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TotalTax { get; set; }
    }

    [XmlRoot(ElementName = "CustomerRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class CustomerRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class BillAddr
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "Line1", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Line1 { get; set; }
        [XmlElement(ElementName = "CountrySubDivisionCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CountrySubDivisionCode { get; set; }
        [XmlElement(ElementName = "PostalCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PostalCode { get; set; }
    }

    [XmlRoot(ElementName = "ShipAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class ShipAddr
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "Line1", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Line1 { get; set; }
        [XmlElement(ElementName = "CountrySubDivisionCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CountrySubDivisionCode { get; set; }
        [XmlElement(ElementName = "PostalCode", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PostalCode { get; set; }
    }

    [XmlRoot(ElementName = "Invoice", Namespace = "http://schema.intuit.com/finance/v3")]
    public class Invoice
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "SyncToken", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SyncToken { get; set; }
        [XmlElement(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
        public MetaData MetaData { get; set; }
        [XmlElement(ElementName = "CustomField", Namespace = "http://schema.intuit.com/finance/v3")]
        public CustomField CustomField { get; set; }
        [XmlElement(ElementName = "DocNumber", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DocNumber { get; set; }
        [XmlElement(ElementName = "TxnDate", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TxnDate { get; set; }
        [XmlElement(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public CurrencyRef CurrencyRef { get; set; }
        [XmlElement(ElementName = "Line", Namespace = "http://schema.intuit.com/finance/v3")]
        public List<Line> Line { get; set; }
        [XmlElement(ElementName = "TxnTaxDetail", Namespace = "http://schema.intuit.com/finance/v3")]
        public TxnTaxDetail TxnTaxDetail { get; set; }
        [XmlElement(ElementName = "CustomerRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public CustomerRef CustomerRef { get; set; }
        [XmlElement(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public BillAddr BillAddr { get; set; }
        [XmlElement(ElementName = "ShipAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public ShipAddr ShipAddr { get; set; }
        [XmlElement(ElementName = "DueDate", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DueDate { get; set; }
        [XmlElement(ElementName = "TotalAmt", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TotalAmt { get; set; }
        [XmlElement(ElementName = "ApplyTaxAfterDiscount", Namespace = "http://schema.intuit.com/finance/v3")]
        public string ApplyTaxAfterDiscount { get; set; }
        [XmlElement(ElementName = "PrintStatus", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PrintStatus { get; set; }
        [XmlElement(ElementName = "EmailStatus", Namespace = "http://schema.intuit.com/finance/v3")]
        public string EmailStatus { get; set; }
        [XmlElement(ElementName = "Balance", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Balance { get; set; }
        [XmlElement(ElementName = "Deposit", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Deposit { get; set; }
        [XmlElement(ElementName = "AllowIPNPayment", Namespace = "http://schema.intuit.com/finance/v3")]
        public string AllowIPNPayment { get; set; }
        [XmlElement(ElementName = "AllowOnlinePayment", Namespace = "http://schema.intuit.com/finance/v3")]
        public string AllowOnlinePayment { get; set; }
        [XmlElement(ElementName = "AllowOnlineCreditCardPayment", Namespace = "http://schema.intuit.com/finance/v3")]
        public string AllowOnlineCreditCardPayment { get; set; }
        [XmlElement(ElementName = "AllowOnlineACHPayment", Namespace = "http://schema.intuit.com/finance/v3")]
        public string AllowOnlineACHPayment { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
        [XmlAttribute(AttributeName = "sparse")]
        public string Sparse { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class GetQBInvoiveResponse 
    {
        [XmlElement(ElementName = "Invoice", Namespace = "http://schema.intuit.com/finance/v3")]
        public Invoice Invoice { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}
