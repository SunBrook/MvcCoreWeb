using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MvcCoreWeb.Tools
{
    public static class XmlUtils
    {
        /// <summary>
        /// Object TO XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T obj, Encoding enc)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                CloseOutput = false,
                Encoding = enc,
                OmitXmlDeclaration = false,
                Indent = true,
            };

            using (MemoryStream ms = new MemoryStream())
            using (XmlWriter xw = XmlWriter.Create(ms, xmlWriterSettings))
            using (var reader = new StreamReader(ms, enc, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true))
            {
                XmlSerializer s = new XmlSerializer(typeof(T));
                s.Serialize(xw, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// XML to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static T DeSerializeObject<T>(string xmlData) where T : class
        {
            StringReader stringReader = new StringReader(xmlData);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            T obj = xmlSerializer.Deserialize(stringReader) as T;
            return obj;
        }
    }
}
