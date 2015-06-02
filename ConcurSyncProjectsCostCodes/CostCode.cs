using System.Collections.Generic;
using System.Data.SqlClient;

namespace ConcurSyncProjectsCostCodes
{
   class CostCode
   {
      public string Entity { get; set; }
      public string Description { get; set; }
      public string ProjectNumber { get; set; }

      public static List<CostCode> FetchCostCodes(DataSource dataSource)
      {
         var costCodeList = new List<CostCode>();

         using (var connection = dataSource.CreateConnection())
         {
            var sqlText = "select pjt_entity, pjt_entity_desc, pjpent.project from PJPENT inner join PJPROJ on pjpent.project = pjproj.project and pjproj.status_pa = 'A'";
            var sqlCommand = new SqlCommand(sqlText, connection);

            SqlDataReader reader = null;
            reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
               var costCode = new CostCode
               {
                  Entity = reader["pjt_entity"].ToString().Trim(),
                  Description = reader["pjt_entity_desc"].ToString(),
                  ProjectNumber = reader["project"].ToString().Trim()
               };

               costCodeList.Add(costCode);
            }

            connection.Close();
         }


         return costCodeList;
      }

   }
}
