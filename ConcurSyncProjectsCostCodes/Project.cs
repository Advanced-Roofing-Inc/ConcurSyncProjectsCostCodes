using System.Collections.Generic;
using System.Data.SqlClient;

namespace ConcurSyncProjectsCostCodes
{
   class Project
   {
      public string Number { get; set; }
      public string Description { get; set; }
      public List<CostCode> CostCodes { get; set; }

      public static List<Project> FetchActiveProjects(DataSource dataSource)
      {
         var projectList = new List<Project>();

         using (var connection = dataSource.CreateConnection())
         {
            var sqlText = "SELECT project, project_desc FROM PJPROJ WHERE status_pa = 'A'";
            var sqlCommand = new SqlCommand(sqlText, connection);

            SqlDataReader reader = null;
            reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
               var project = new Project
               {
                  Number = reader["project"].ToString().Trim(),
                  Description = reader["project_desc"].ToString(),
                  CostCodes = new List<CostCode>()
               };

               projectList.Add(project);
            }

            connection.Close();
         }

         return projectList;
      }
   
      public static void MapCostCodesToProjectList(List<Project> activeProjectList,List<CostCode> costCodeList)
      {
 	      foreach (var cc in costCodeList)
         {
            foreach (var p in activeProjectList)
            {
               if (cc.ProjectNumber.Equals(p.Number))
               {
                  p.CostCodes.Add(cc);
               }
            }
         }
      }

      public static ListItems CreateListToDelete(List<Project> oldItems, List<Project> newItems)
      {
         var listItems = new ListItems { Items = new List<ListItem>() };

         foreach (var oldItem in oldItems)
         {
            if (!newItems.Exists(p => p.Number == oldItem.Number))
            {
               listItems.Items.Add(new ListItem { Level1Code = oldItem.Number });
            }
         }

         return listItems;
      }

      public static ListItems CreateListToAdd(List<Project> oldItems, List<Project> newItems)
      {
         var itemsToAdd = new ListItems { Items = new List<ListItem>() };

         foreach (var newItem in newItems)
         {
            if (!oldItems.Exists(p => p.Number == newItem.Number))
            {
               var listItem = new ListItem
               {
                  Level1Code = newItem.Number,
                  Name = newItem.Description
               };

               itemsToAdd.Items.Add(listItem);
            }
         }

         return itemsToAdd;
      }

      public List<ListItem> GetCostCodesToDelete(List<CostCode> oldCostCodes)
      {
         var costCodesToDelete = new List<ListItem>();

         foreach (var costCode in oldCostCodes)
         {
            if (!this.CostCodes.Exists(cc => cc.Entity == costCode.Entity))
            {
               var listItem = new ListItem
               {
                  Level1Code = costCode.ProjectNumber,
                  Level2Code = costCode.Entity
               };

               costCodesToDelete.Add(listItem);
            }
         }

         return costCodesToDelete;
      }


      public List<ListItem> GetCostCodesToAdd(List<CostCode> oldCostCodes)
      {
         var costCodesToAdd = new List<ListItem>();

         foreach (var costCode in this.CostCodes)
         {
            if (!oldCostCodes.Exists(cc => cc.Entity == costCode.Entity))
            {
               var listItem = new ListItem
               {
                  Level1Code = costCode.ProjectNumber,
                  Level2Code = costCode.Entity,
                  Name = costCode.Description
               };

               costCodesToAdd.Add(listItem);
            }
         }

         return costCodesToAdd;
      }
   }
}
