using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace VT.QuickBooks.DTOs.CompanyServices
{
    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class GetQBServiceResponse: QuickbookBaseResponse
    {
        [XmlElement(ElementName = "Item", Namespace = "http://schema.intuit.com/finance/v3")]
        public Item Item { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}
