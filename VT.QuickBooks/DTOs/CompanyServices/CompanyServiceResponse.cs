using System.Xml.Serialization;
namespace VT.QuickBooks.DTOs.CompanyServices
{
    [XmlRoot(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
    public class MetaData
    {
        [XmlElement(ElementName = "CreateTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string CreateTime { get; set; }
        [XmlElement(ElementName = "LastUpdatedTime", Namespace = "http://schema.intuit.com/finance/v3")]
        public string LastUpdatedTime { get; set; }
    }

    [XmlRoot(ElementName = "IncomeAccountRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class IncomeAccountRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "ExpenseAccountRef", Namespace = "http://schema.intuit.com/finance/v3")]
    public class ExpenseAccountRef
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Item", Namespace = "http://schema.intuit.com/finance/v3")]
    public class Item
    {
        [XmlElement(ElementName = "Id", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Id { get; set; }
        [XmlElement(ElementName = "SyncToken", Namespace = "http://schema.intuit.com/finance/v3")]
        public string SyncToken { get; set; }
        [XmlElement(ElementName = "MetaData", Namespace = "http://schema.intuit.com/finance/v3")]
        public MetaData MetaData { get; set; }
        [XmlElement(ElementName = "Name", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Active", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Active { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "FullyQualifiedName", Namespace = "http://schema.intuit.com/finance/v3")]
        public string FullyQualifiedName { get; set; }
        [XmlElement(ElementName = "Taxable", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Taxable { get; set; }
        [XmlElement(ElementName = "UnitPrice", Namespace = "http://schema.intuit.com/finance/v3")]
        public string UnitPrice { get; set; }
        [XmlElement(ElementName = "Type", Namespace = "http://schema.intuit.com/finance/v3")]
        public string Type { get; set; }
        [XmlElement(ElementName = "IncomeAccountRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public IncomeAccountRef IncomeAccountRef { get; set; }
        [XmlElement(ElementName = "PurchaseCost", Namespace = "http://schema.intuit.com/finance/v3")]
        public string PurchaseCost { get; set; }
        [XmlElement(ElementName = "ExpenseAccountRef", Namespace = "http://schema.intuit.com/finance/v3")]
        public ExpenseAccountRef ExpenseAccountRef { get; set; }
        [XmlElement(ElementName = "TrackQtyOnHand", Namespace = "http://schema.intuit.com/finance/v3")]
        public string TrackQtyOnHand { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
        [XmlAttribute(AttributeName = "sparse")]
        public string Sparse { get; set; }
    }

    [XmlRoot(ElementName = "IntuitResponse", Namespace = "http://schema.intuit.com/finance/v3")]
    public class GetCompanyServiceResponse
    {
        [XmlElement(ElementName = "Item", Namespace = "http://schema.intuit.com/finance/v3")]
        public Item Item { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}
