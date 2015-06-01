using System;
using System.Collections.Generic;
using System.Configuration;

namespace ConcurSyncProjectsCostCodes
{
   class Program
   {
      static void Main(string[] args)
      {
         // Connect to SL database
         Console.WriteLine("Connecting to SL database... ");
         var connectionString = ConfigurationManager.ConnectionStrings["SL"].ConnectionString;
         var dataSource = new DataSource(connectionString);

         // Build list of active projects with their cost codes
         var activeProjectList = Project.FetchActiveProjects(dataSource);
         var costCodeList = CostCode.FetchCostCodes(dataSource);
         Project.MapCostCodesToProjectList(activeProjectList, costCodeList);

         // Connect to Concur service
         Console.WriteLine("Getting access token from Concur service...");
         var username = ConfigurationManager.AppSettings["ConcurUsername"];
         var password = ConfigurationManager.AppSettings["ConcurPassword"];
         var consumerKey = ConfigurationManager.AppSettings["ConcurConsumerKey"];

         // Get access token from Concur API
         var concur = new ConcurApi(username, password, consumerKey);

         var concurProjectListUrl = ConfigurationManager.AppSettings["ProjectListUrl"];

         // Fetch list of projects currently in Concur
         Console.WriteLine("Fetching list of projects from Concur API...");
         var concurProjectList = concur.FetchProjectList(concurProjectListUrl);

         // Find projects to be deleted
         var projectsToDelete = Project.CreateListToDelete(concurProjectList, activeProjectList);

         var response = concur.DeleteItems(concurProjectListUrl, projectsToDelete);
         Console.WriteLine(response.Content);

         Console.ReadKey();
      }

   }
}
