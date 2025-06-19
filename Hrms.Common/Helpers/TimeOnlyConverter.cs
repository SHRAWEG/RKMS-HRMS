using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hrms.Common.Helpers
{
    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private readonly string serializationFormat;

        public TimeOnlyConverter() : this(null)
        {
        }

        public TimeOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "hh:mm:ss";
        }

        public override TimeOnly Read(ref Utf8JsonReader reader,
                                Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value,
                                            JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
        }
    }
