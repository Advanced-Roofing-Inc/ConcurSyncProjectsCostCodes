using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using System.Collections.Generic;
using System.Net;

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

         // Force TLS 1.2, default TLS 1.0 support dropped Oct 2017
         ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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

      public IRestResponse Batch(string listUrl, ListItems items, string action)
      {
         var body = items.Serialize();

         var client = new RestClient(listUrl);
         var request = new RestRequest("batch", Method.POST);
         request.AddQueryParameter("type", action);
         request.AddHeader("Accept", "application/xml");
         request.AddHeader("Authorization", "OAuth " + this.accessToken.Token);
         request.AddParameter("application/xml", body, ParameterType.RequestBody);

         return client.Execute(request);
      }

   }
}
