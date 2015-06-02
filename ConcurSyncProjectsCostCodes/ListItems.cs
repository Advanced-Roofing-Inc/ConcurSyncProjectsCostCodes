using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ConcurSyncProjectsCostCodes
{
   [XmlRoot("list-item-batch", Namespace = "http://www.concursolutions.com/api/expense/list/2010/02")]
   public class ListItems
   {
      [XmlElement("list-item")]
      public List<ListItem> Items { get; set; }

      public string Serialize()
      {
         XmlSerializer serializer = new XmlSerializer(typeof(ListItems));
         XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
         namespaces.Add("", "http://www.concursolutions.com/api/expense/list/2010/02");
         var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
         MemoryStream ms = new MemoryStream();
         XmlWriter writer = XmlWriter.Create(ms, settings);
         serializer.Serialize(writer, this, namespaces);
         ms.Flush();
         ms.Seek(0, SeekOrigin.Begin);
         StreamReader sr = new StreamReader(ms);
         var body = sr.ReadToEnd();

         return body;
      }
   }
}
