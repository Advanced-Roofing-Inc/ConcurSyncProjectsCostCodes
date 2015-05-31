using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using RestSharp;
using RestSharp.Deserializers;
using System.Collections.Generic;

namespace ConcurSyncProjectsCostCodes
{
   class Program
   {
      static void Main(string[] args)
      {
         // Connect to SL database
         Console.Write("Connecting to SL database... ");
         var connectionString = ConfigurationManager.ConnectionStrings["SL"].ConnectionString;
         var dataSource = new DataSource(connectionString);

         // Build list of active projects with their cost codes
         var activeProjectList = Project.FetchActiveProjects(dataSource);
         var costCodeList = CostCode.FetchCostCodes(dataSource);
         Project.MapCostCodesToProjectList(activeProjectList, costCodeList);

         // Connect to Concur service
         Console.Write("Getting access token from Concur service...");
         var concurUsername = ConfigurationManager.AppSettings["ConcurUsername"];
         var concurPassword = ConfigurationManager.AppSettings["ConcurPassword"];
         var consumerKey = ConfigurationManager.AppSettings["ConcurConsumerKey"];

         var uri = "http://www.concursolutions.com/api/expense/list/v1.0/";

         var client = new RestClient("https://www.concursolutions.com/net2/oauth2/accesstoken.ashx");
         client.Authenticator = new HttpBasicAuthenticator(concurUsername, concurPassword);
         client.AddDefaultHeader("X-ConsumerKey", consumerKey);
         
         var request = new RestRequest("", Method.GET);
         request.AddHeader("Accept", "application/xml");
         
         var response = client.Execute(request);
         var deserializer = new XmlDeserializer();
         AccessToken token = deserializer.Deserialize<AccessToken>(response);

         Console.ReadKey();
      }

   }
}
