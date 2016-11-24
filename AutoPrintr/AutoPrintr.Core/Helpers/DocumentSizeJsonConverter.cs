using AutoPrintr.Core.Models;
using Newtonsoft.Json;
using System;

namespace AutoPrintr.Core.Helpers
{
    public class DocumentSizeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DocumentSize);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var documentSize = DocumentSize.Letter;
            var documentSizeString = reader.Value.ToString().Replace(" ", string.Empty).Replace("-", string.Empty);

            if (!Enum.TryParse(documentSizeString, true, out documentSize))
                throw new InvalidOperationException($"{nameof(DocumentSizeJsonConverter)} exception: cannot deserialize value of '{documentSizeString}' into {nameof(DocumentSize)} type");

            return documentSize;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var documentSize = (DocumentSize)value;
            writer.WriteValue(documentSize.ToString());
        }
    }
}