using RestSharp.Deserializers;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace ConcurSyncProjectsCostCodes
{
   [XmlRoot("list-item-batch", Namespace = "http://www.concursolutions.com/api/expense/list/2010/02")]
   public class ListItems
   {
      [XmlElement("list-item")]
      public List<ListItem> Items { get; set; }
   }
}
