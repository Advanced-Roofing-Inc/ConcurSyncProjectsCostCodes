using System.Configuration;
using System.Data.SqlClient;

namespace ConcurSyncProjectsCostCodes
{
   class Program
   {
      static void Main(string[] args)
      {
         var connectionString = ConfigurationManager.ConnectionStrings["SL"].ConnectionString;
         var dataSource = new SqlConnection(connectionString);
      }
   }
}
