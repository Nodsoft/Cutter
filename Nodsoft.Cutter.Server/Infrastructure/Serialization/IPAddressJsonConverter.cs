using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nodsoft.Cutter.Server.Infrastructure.Serialization;

/// <summary>
/// Converts IP addresses to and from JSON.
/// </summary>
public class IPAddressJsonConverter : JsonConverter<IPAddress>
{
    public override IPAddress? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) 
        => reader.GetString() is { } value 
            ? IPAddress.Parse(value) 
            : null;

    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options) 
        => writer.WriteStringValue(value.ToString());
}