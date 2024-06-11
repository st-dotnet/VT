using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace VT.QuickBooks.DTOs.CompanyServices
{
    [XmlRoot(ElementName = "AssetAccountRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class AssetAccountRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "QueryResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class QueryResponse
    {
        [XmlElement(ElementName = "Item", Namespace = "http://schema.intuit.com/finance/v3")]
        public List<Item> Item { get; set; }
        [XmlAttribute(AttributeName = "startPosition")]
        public string StartPosition { get; set; }
        [XmlAttribute(AttributeName = "maxResults")]
        public string MaxResults { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class GetAllServices : QuickbookBaseResponse
    {
        [XmlElement(ElementName = "QueryResponse", Namespace = "http://schema.intuit.com/finance/v3")]
        public QueryResponse QueryResponse { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }
}