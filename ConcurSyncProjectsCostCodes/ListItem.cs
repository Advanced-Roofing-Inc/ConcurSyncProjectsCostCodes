using System.Xml.Serialization;

namespace ConcurSyncProjectsCostCodes
{
   public class ListItem
   {
      public string ItemsLink { get; set; }

      [XmlElement("name")]
      public string Name { get; set; }

      [XmlElement("level1code")]
      public string Level1Code { get; set; }

      [XmlElement("level2code")]
      public string Level2Code { get; set; }

      [XmlElement("child-count")]
      public int ChildCount { get; set; }
   }
}
