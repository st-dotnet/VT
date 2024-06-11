using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace VT.QuickBooks.DTOs.Employee
{
    [XmlRoot(ElementName = "QueryResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class QueryResponse
    {
        [XmlElement(ElementName = "Employee", Namespace = "http://schema.intuit.com/finance/v3")]
        public List<Employee> Employees { get; set; }
        [XmlAttribute(AttributeName = "startPosition")]
        public string StartPosition { get; set; }
        [XmlAttribute(AttributeName = "maxResults")]
        public string MaxResults { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class GetAllEmployees:QuickbookBaseResponse
    {
        [XmlElement(ElementName = "QueryResponse", Namespace = "http://schema.intuit.com/finance/v3")]
        public QueryResponse QueryResponse { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}