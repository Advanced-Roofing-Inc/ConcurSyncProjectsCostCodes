using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurSyncProjectsCostCodes
{
   class DataSource
   {
      private String _connectionString { get; set; }

      public DataSource(String connectionString)
      {
         this._connectionString = connectionString;
      }

      public SqlConnection CreateConnection()
      {
         var connection = new SqlConnection(this._connectionString);

         try
         {
            connection.Open();
         }
         catch (Exception e)
         {
            Console.WriteLine(e.ToString());
         }

         return connection;
      }
   }
}
