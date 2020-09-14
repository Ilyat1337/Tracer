using System.Text.Json;

namespace Demonstration.Serialization
{
    class JSONSerializer : ISerializer
    {
        public string Serizlize(object o)
        {
            var options = GetJsonSerializerOptions();
            return JsonSerializer.Serialize(o, o.GetType(), options);
        }

        private JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }
    }
}
