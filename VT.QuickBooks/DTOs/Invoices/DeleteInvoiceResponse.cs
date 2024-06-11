using System;

using System.Xml.Serialization;
using System.Collections.Generic;

namespace VT.QuickBooks.DTOs.Invoices
{
    [XmlRoot(ElementName = "Invoice", Namespace = "http://schema.intuit.com/finance/v3")]
    public class DeleteInvoice
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class DeleteInvoiceResponse
    {
        [XmlElement(ElementName = "Invoice", Namespace = "http://schema.intuit.com/finance/v3")]
        public DeleteInvoice Invoice { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }
}