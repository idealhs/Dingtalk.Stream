using System.Text.Json;

namespace Dingtalk.Stream.Entities
{
    public class ProtocolDto
    {
        public string SpecVersion { get; set; }

        public string Type { get; set; }

        public ProtocolHeadersDto Headers { get; set; }

        public string Data { get; set; }

        public JsonDocument GetDataAsJson()
        {
            return JsonDocument.Parse(Data);
        }
    }

    public class ProtocolHeadersDto
    {
        public string Topic { get; set; }

        public string ContentType { get; set; }

        public string MessageId { get; set; }

        public long Time { get; set; }
    }
}