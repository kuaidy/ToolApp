using System.Text.Json;
using System;
using System.Text.Json.Serialization;

namespace ToolApp.Common
{
    public class JsonDateTimeConverter: JsonConverter<DateTime>
    {
        private readonly string m_Format = "yyyy-MM-dd HH:mm:ss";
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (DateTime.TryParseExact(reader.GetString(), m_Format, null, System.Globalization.DateTimeStyles.None, out var dateTime))
            {
                return dateTime;
            }
            return reader.GetDateTime(); // fallback for ISO 8601
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(m_Format));
        }
    }
}
