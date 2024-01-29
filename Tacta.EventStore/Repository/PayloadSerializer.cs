using Newtonsoft.Json;

namespace Tacta.EventStore.Repository
{
    public static class PayloadSerializer
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        };

        public static string Serialize<T>(T @event)
        {
            return JsonConvert.SerializeObject(@event, Formatting.Indented, JsonSerializerSettings);
        }
        
        internal static T Deserialize<T>(StoredEvent @event)
        {
            return JsonConvert.DeserializeObject<T>(@event.Payload, JsonSerializerSettings);
        }
    }
}