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

         // Fetch cost codes currently in Concur
         Console.WriteLine("Fetching cost codes for existing projects...");
         int i = 0;
         foreach (var oldProject in concurProjectList)
         {
            ClearCurrentConsoleLine();
            double percentComplete = ((double) ++i / (double) concurProjectList.Count) * 100.0;
            Console.Write("{0} - {1}%", oldProject.Number, Math.Ceiling(percentComplete));
            oldProject.CostCodes = concur.FetchCostCodesForProject(concurProjectListUrl, oldProject.Number);
         }

         // Find projects to be deleted
         var projectsToDelete = Project.CreateListToDelete(concurProjectList, activeProjectList);
         Console.WriteLine("\nFound {0} projects to be deleted.", projectsToDelete.Items.Count);

         // Delete items from concur
         if (projectsToDelete.Items.Count > 0)
         {
            Console.Write("Deleting projects from Concur...");
            var deleteResponse = concur.Batch(concurProjectListUrl, projectsToDelete, "delete");
            Console.WriteLine(deleteResponse.ResponseStatus.ToString());
         }         
         
         // Find projects to be added
         var projectsToAdd = Project.CreateListToAdd(concurProjectList, activeProjectList);
         Console.WriteLine("Found {0} projects to be added.", projectsToAdd.Items.Count);

         var foo = concurProjectList.Find(c => c.Number == "15OS11");

         // Add items to Concur
         if (projectsToAdd.Items.Count > 0)
         {
            Console.Write("Adding projects to Concur...");
            var createResponse = concur.Batch(concurProjectListUrl, projectsToAdd, "create");
            Console.WriteLine(createResponse.ResponseStatus.ToString());
         }

         // Find cost codes to add and remove
         var costCodesToDelete = new ListItems { Items = new List<ListItem>() };
         var costCodesToAdd = new ListItems { Items = new List<ListItem>() };
         foreach (var project in activeProjectList)
         {
            var oldProject = concurProjectList.Find(p => p.Number == project.Number);

            if (oldProject != null)
            {
               var deleteables = project.GetCostCodesToDelete(oldProject.CostCodes);
               var addables = project.GetCostCodesToAdd(oldProject.CostCodes);

               foreach (var cc in deleteables)
               {
                  costCodesToDelete.Items.Add(cc);
               }

               foreach (var cc in addables)
               {
                  costCodesToAdd.Items.Add(cc);
               }
            }
            else
            {
               // Project is new, so all cost codes must be added
               foreach (var costCode in project.CostCodes)
               {
                  costCodesToAdd.Items.Add(new ListItem
                  {
                     Level1Code = costCode.ProjectNumber,
                     Level2Code = costCode.Entity,
                     Name = costCode.Description
                  });
               }
            }
         }

         Console.WriteLine("Found {0} cost codes to be added, {1} to be deleted.", costCodesToAdd.Items.Count, costCodesToDelete.Items.Count);

         if (costCodesToDelete.Items.Count > 0)
         {
            Console.Write("Deleting old cost codes from Concur... ");
            var deleteCcResponse = concur.Batch(concurProjectListUrl, costCodesToDelete, "delete");
            Console.WriteLine(deleteCcResponse.ResponseStatus.ToString());
         }
         
         if (costCodesToAdd.Items.Count > 0)
         {
            Console.Write("Adding new cost codes to Concur... ");
            var addCcResponse = concur.Batch(concurProjectListUrl, costCodesToAdd, "create");
            Console.WriteLine(addCcResponse.ResponseStatus.ToString());
         }
      }

      public static void ClearCurrentConsoleLine()
      {
         int currentLineCursor = Console.CursorTop;
         Console.SetCursorPosition(0, Console.CursorTop);
         for (int i = 0; i < Console.WindowWidth; i++)
            Console.Write(" ");
         Console.SetCursorPosition(0, currentLineCursor);
      }

   }
}
