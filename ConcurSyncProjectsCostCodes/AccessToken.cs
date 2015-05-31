using RestSharp.Deserializers;

namespace ConcurSyncProjectsCostCodes
{
   class AccessToken
   {
      public string InstanceUrl { get; set; }

      public string Token { get; set; }

      [DeserializeAs(Name = "Expiration_date")]
      public string ExpirationDate { get; set; }

      public string RefreshToken { get; set; }
   }
}
