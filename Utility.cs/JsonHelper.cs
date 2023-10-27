using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Utility.cs
{
    public class JsonHelper
    {
        public static T? DeserializeObject<T>(string json)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
        }
    }
}