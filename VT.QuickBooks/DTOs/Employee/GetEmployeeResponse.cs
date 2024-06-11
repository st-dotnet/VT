using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace VT.QuickBooks.DTOs.Employee
{
    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class GetEmployeeResponse : QuickbookBaseResponse
    {
        [XmlElement(ElementName = "Employee", Namespace = "http://schema.intuit.com/finance/v3")]
        public Employee Employee { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}
