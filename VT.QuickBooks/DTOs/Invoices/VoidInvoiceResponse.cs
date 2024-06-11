using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace VT.QuickBooks.DTOs.Invoices
{
    [XmlRoot(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidMetaData
    {
        [XmlElement(ElementName = "CreateTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CreateTime { get; set; }
        [XmlElement(ElementName = "LastUpdatedTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string LastUpdatedTime { get; set; }
    }

    [XmlRoot(ElementName = "CustomField", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidCustomField
    {
        [XmlElement(ElementName = "DefinitionId", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DefinitionId { get; set; }
        [XmlElement(ElementName = "Name", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Type", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidCurrencyRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ItemRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidItemRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "SalesItemLineDetail", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidInvoiceSalesItemLineDetail
    {
        [XmlElement(ElementName = "ItemRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidItemRef ItemRef { get; set; }
        [XmlElement(ElementName = "TaxCodeRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TaxCodeRef { get; set; }
    }

    [XmlRoot(ElementName = "Line", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidInvoiceLine
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "LineNum", Namespace = "http://schema.intuit.com/finance/v3")]
        public string LineNum { get; set; }
        [XmlElement(ElementName = "Amount", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Amount { get; set; }
        [XmlElement(ElementName = "DetailType", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DetailType { get; set; }
        [XmlElement(ElementName = "SalesItemLineDetail", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidInvoiceSalesItemLineDetail SalesItemLineDetail { get; set; }
        [XmlElement(ElementName = "SubTotalLineDetail", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SubTotalLineDetail { get; set; }
    }

    [XmlRoot(ElementName = "TxnTaxDetail", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidInvoiceTxnTaxDetail
    {
        [XmlElement(ElementName = "TotalTax", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TotalTax { get; set; }
    }

    [XmlRoot(ElementName = "CustomerRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidInvoiceCustomerRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidInvoiceBillAddr
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

    [XmlRoot(ElementName = "Invoice", Namespace = "http://schema.intuit.com/finance/v3")]
    public class VoidInvoice
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "SyncToken", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SyncToken { get; set; }
        [XmlElement(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidMetaData MetaData { get; set; }
        [XmlElement(ElementName = "CustomField", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidCustomField CustomField { get; set; }
        [XmlElement(ElementName = "DocNumber", Namespace = "http://schema.intuit.com/finance/v3")]
        public string DocNumber { get; set; }
        [XmlElement(ElementName = "TxnDate", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TxnDate { get; set; }
        [XmlElement(ElementName = "CurrencyRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidCurrencyRef CurrencyRef { get; set; }
        [XmlElement(ElementName = "PrivateNote", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PrivateNote { get; set; }
        [XmlElement(ElementName = "Line", Namespace = "http://schema.intuit.com/finance/v3")]
        public List<VoidInvoiceLine> Line { get; set; }
        [XmlElement(ElementName = "TxnTaxDetail", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidInvoiceTxnTaxDetail TxnTaxDetail { get; set; }
        [XmlElement(ElementName = "CustomerRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidInvoiceCustomerRef CustomerRef { get; set; }
        [XmlElement(ElementName = "BillAddr", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidInvoiceBillAddr BillAddr { get; set; }
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
    public class VoidInvoiceResponse
    {
        [XmlElement(ElementName = "Invoice", Namespace = "http://schema.intuit.com/finance/v3")]
        public VoidInvoice Invoice { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}

