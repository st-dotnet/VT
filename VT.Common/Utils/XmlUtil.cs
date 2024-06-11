using System.IO;
using System.Xml.Serialization;

namespace VT.Common.Utils
{
    public static class XmlUtil
    {
        public static T Deserialize<T>(string input) where T : class
        {
            var ser = new XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        public static string Serialize<T>(T ObjectToSerialize)
        {
            var xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                return textWriter.ToString();
            }
        }
    }
}
