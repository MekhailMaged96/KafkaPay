using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace KafkaPay.Shared.Domain.Entities
{
    public class OutBoxMessage
    {
        public Guid Id { get; private set; }
        public string Type { get; private set; }
        public string Content { get; private set; }
        public DateTime OccuredOnUtc { get; private set; }
        public DateTime? ProcessedOnUtc { get; private set; }
        public string? Error { get; private set; }

        public OutBoxMessage()
        {
            
        }
        public OutBoxMessage(string type, object content, DateTime occuredOnUtc)
        {
            Type = type;
            Content = JsonConvert.SerializeObject(content,new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            OccuredOnUtc = occuredOnUtc;
        }

        public void MarkAsProcessed(DateTime processedOnUtc) => ProcessedOnUtc = processedOnUtc;
        public void MarkAsFailed(string error) => Error = error;

        //public T DeserializeContent<T>() => JsonConvert.DeserializeObject<T>(Content)!;
    }
}
