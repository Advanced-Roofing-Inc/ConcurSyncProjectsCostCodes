using RestSharp;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace ConcurSyncProjectsCostCodes
{
   class ConcurApi
   {
      private AccessToken accessToken;

      public ConcurApi(string username, string password, string consumerKey)
      {
         var client = new RestClient("https://www.concursolutions.com/net2/oauth2/accesstoken.ashx");
         client.Authenticator = new HttpBasicAuthenticator(username, password);
         client.AddDefaultHeader("X-ConsumerKey", consumerKey);

         var request = new RestRequest();
         request.AddHeader("Accept", "application/xml");

         var response = client.Execute(request);
         var deserializer = new XmlDeserializer();
         AccessToken token = deserializer.Deserialize<AccessToken>(response);

         this.accessToken = token;
      }

      public List<Project> FetchProjectList(string listUrl)
      {
         var projectList = new List<Project>();

         var client = new RestClient(listUrl);
         var request = new RestRequest("items", Method.GET);
         request.AddHeader("Accept", "application/xml");
         request.AddHeader("Authorization", "OAuth " + this.accessToken.Token);

         var response = client.Execute(request);
         var deserializer = new XmlDeserializer();
         var listItems = deserializer.Deserialize<ListItems>(response);
        
         foreach (var item in listItems.Items)
         {
            var project = new Project
            {
               Number = item.Level1Code,
               Description = item.Name,
               CostCodes = new List<CostCode>()
            };

            projectList.Add(project);
         }

         return projectList;
      }

      public List<CostCode> FetchCostCodesForProject(string listUrl, string projectNumber)
      {
         var costCodeList = new List<CostCode>();

         var client = new RestClient(listUrl);
         var request = new RestRequest("items", Method.GET);
         request.AddQueryParameter("parentCode", projectNumber);
         request.AddHeader("Accept", "application/xml");
         request.AddHeader("Authorization", "OAuth " + this.accessToken.Token);

         var response = client.Execute(request);
         var deserializer = new XmlDeserializer();
         var listItems = deserializer.Deserialize<List<ListItem>>(response);

         foreach (var item in listItems)
         {
            var costCode = new CostCode
            {
               Entity = item.Level2Code.Trim(),
               Description = item.Name,
               ProjectNumber = item.Level1Code.Trim()
            };

            costCodeList.Add(costCode);
         }

         return costCodeList;
      }

      public IRestResponse DeleteItems(string listUrl, ListItems items)
      {
         XmlSerializer serializer = new XmlSerializer(typeof(ListItems));
         XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
         namespaces.Add("", "http://www.concursolutions.com/api/expense/list/2010/02");
         var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
         MemoryStream ms = new MemoryStream();
         XmlWriter writer = XmlWriter.Create(ms, settings);
         serializer.Serialize(writer, items, namespaces);
         ms.Flush();
         ms.Seek(0, SeekOrigin.Begin);
         StreamReader sr = new StreamReader(ms);
         var body = sr.ReadToEnd();

         var client = new RestClient(listUrl);
         var request = new RestRequest("batch", Method.POST);
         request.AddQueryParameter("type", "delete");
         request.AddHeader("Accept", "application/xml");
         request.AddHeader("Authorization", "OAuth " + this.accessToken.Token);
         request.AddParameter("application/xml", body, ParameterType.RequestBody);

         var response = client.Execute(request);

         return response;
      }
   }
}
